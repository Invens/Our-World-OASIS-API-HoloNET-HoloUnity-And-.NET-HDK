﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NextGenSoftware.OASIS.API.DNA;
using NextGenSoftware.OASIS.API.DNA.Manager;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Events;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Holons;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using NextGenSoftware.OASIS.API.Core.Managers;
using NextGenSoftware.OASIS.API.Core.Objects;
using NextGenSoftware.OASIS.STAR.CelestialBodies;
using NextGenSoftware.OASIS.STAR.ExtensionMethods;
using NextGenSoftware.OASIS.STAR.DNA;
using NextGenSoftware.OASIS.STAR.OASISAPIManager;
using NextGenSoftware.OASIS.STAR.Zomes;
using NextGenSoftware.OASIS.STAR.ErrorEventArgs;
using NextGenSoftware.Holochain.HoloNET.Client.Core;

namespace NextGenSoftware.OASIS.STAR
{
    public static class SuperStar
    {
        const string STAR_DNA_DEFAULT_PATH = "DNA\\STAR_DNA.json";
        const string OASIS_DNA_DEFAULT_PATH = "DNA\\OASIS_DNA.json";

        private static OASISAPI _OASISAPI = null;

        public static string STARDNAPath { get; set; } = STAR_DNA_DEFAULT_PATH;
        public static string OASISDNAPath { get; set; } = OASIS_DNA_DEFAULT_PATH;
        public static STARDNA STARDNA { get; set; }

        public static OASISDNA OASISDNA
        {
            get
            {
                return OASISDNAManager.OASISDNA;
            }
        }

        public static bool IsSuperStarIgnited
        { 
            get
            {
                return SuperStarCore != null;
            }
        }

        public static Star InnerStar { get; set; }
        public static SuperStarCore SuperStarCore { get; set; }
        public static List<Star> Stars { get; set; }
        public static List<Planet> Planets { get; set; }
        public static Avatar LoggedInUser { get; set; }

        public static OASISAPI OASISAPI
        {
            get
            {
                if (_OASISAPI == null)
                    _OASISAPI = new OASISAPI();

                return _OASISAPI;
            }
        }
        
        public delegate void HolonsLoaded(object sender, HolonsLoadedEventArgs e);
        public static event HolonsLoaded OnHolonsLoaded;

        public delegate void ZomesLoaded(object sender, ZomesLoadedEventArgs e);
        public static event ZomesLoaded OnZomesLoaded;

        public delegate void HolonSaved(object sender, HolonSavedEventArgs e);
        public static event HolonSaved OnHolonSaved;

        public delegate void HolonLoaded(object sender, HolonLoadedEventArgs e);
        public static event HolonLoaded OnHolonLoaded;

        public delegate void Initialized(object sender, EventArgs e);
        public static event Initialized OnInitialized;

        public delegate void ZomeError(object sender, ZomeErrorEventArgs e);
        public static event ZomeError OnZomeError;

        public delegate void StarError(object sender, StarErrorEventArgs e);
        public static event StarError OnStarError;

        //TODO: Not sure if we want to expose the HoloNETClient events at this level? They can subscribe to them through the HoloNETClient property below...
        public delegate void Disconnected(object sender, DisconnectedEventArgs e);
        public static event Disconnected OnDisconnected;

        public delegate void DataReceived(object sender, DataReceivedEventArgs e);
        public static event DataReceived OnDataReceived;

        public static OASISResult<ICelestialBody> IgniteSuperStar()
        {
            return IgniteSuperStar(InitOptions.InitWithCurrentDefaultProvider);
        }

        public static async Task<OASISResult<ICelestialBody>> IgniteSuperStarAsync()
        {
            return await IgniteSuperStarAsync(InitOptions.InitWithCurrentDefaultProvider);
        }

        //public static OASISResult<ICelestialBody> IgniteSuperStar(InitOptions OASISAPIInitOptions, string STARDNAPath = STAR_DNA_DEFAULT_PATH, string OASISDNAPath = OASIS_DNA_DEFAULT_PATH, Dictionary<ProviderType, string> starProviderKey = null)
        public static OASISResult<ICelestialBody> IgniteSuperStar(InitOptions OASISAPIInitOptions, string STARDNAPath = STAR_DNA_DEFAULT_PATH, string OASISDNAPath = OASIS_DNA_DEFAULT_PATH, string starId = null)
        {
            OASISResult<ICelestialBody> result = IgniteSuperStarInternal(OASISAPIInitOptions, STARDNAPath, OASISDNAPath, starId);

            if (!result.IsError && InnerStar.Id == Guid.Empty)
            {
                //TODO: Implement Save method (non async version) and call instead of below:
                result = InnerStar.SaveAsync().Result;

                if (!result.IsError && result.IsSaved)
                {
                    result.Message = "SuperSTAR Ignited";
                    STARDNA.StarId = InnerStar.Id.ToString(); 
                    SaveDNA();
                }
            }

            return result;
        }

        public static async Task<OASISResult<ICelestialBody>> IgniteSuperStarAsync(InitOptions OASISAPIInitOptions, string STARDNAPath = STAR_DNA_DEFAULT_PATH, string OASISDNAPath = OASIS_DNA_DEFAULT_PATH, string starId = null)
        {
            OASISResult<ICelestialBody> result = IgniteSuperStarInternal(OASISAPIInitOptions, STARDNAPath, OASISDNAPath, starId);

            if (!result.IsError && InnerStar.Id == Guid.Empty)
            {
                result = await InnerStar.SaveAsync();

                if (!result.IsError && result.IsSaved)
                {
                    result.Message = "SuperSTAR Ignited";
                    STARDNA.StarId = InnerStar.Id.ToString(); //TODO: May just store this internally by adding a LoadSuperStar method which would call LoadHolon passing in HolonType SuperStar (depends if if there will be more than one SuperStar in future? ;-) ) Maybe for distributing so can easier handle load? It's one SuperStar per Galaxy so could have more than one Galaxy? So The OASIS and COSMIC would be a full Universe with multiple Galaxies with their own SuperStar in the centre... ;-) YES! But would we need a GrandSuperStar then? For the centre of the Universe? Which will connect to other Universes and creates SuperStars? Or could a SuperStar just create other SuperStars? :) Yes think better to just for now allow SuperStar to create other SuperStars... ;-)
                    SaveDNA();
                }
            }

            return result;
        }

        private static void WireUpEvents()
        {
            SuperStarCore.OnHolonLoaded += SuperStarCore_OnHolonLoaded;
            SuperStarCore.OnHolonSaved += SuperStarCore_OnHolonSaved;
            SuperStarCore.OnHolonsLoaded += SuperStarCore_OnHolonsLoaded;
            SuperStarCore.OnZomeError += SuperStarCore_OnZomeError;
            SuperStarCore.OnInitialized += SuperStarCore_OnInitialized;
        }

        private static void SuperStarCore_OnInitialized(object sender, EventArgs e)
        {
            OnInitialized?.Invoke(sender, e);
        }

        private static void SuperStarCore_OnZomeError(object sender, ZomeErrorEventArgs e)
        {
            OnZomeError?.Invoke(sender, e);
        }

        private static void SuperStarCore_OnHolonLoaded(object sender, HolonLoadedEventArgs e)
        {
            OnHolonLoaded?.Invoke(sender, e);
        }

        private static void SuperStarCore_OnHolonSaved(object sender, HolonSavedEventArgs e)
        {
            OnHolonSaved?.Invoke(sender, e);
        }

        private static void SuperStarCore_OnHolonsLoaded(object sender, HolonsLoadedEventArgs e)
        {
            OnHolonsLoaded?.Invoke(sender, e);
        }

        public static async Task<OASISResult<IAvatar>> BeamInAsync(string username, string password)
        {
            string hostName = Dns.GetHostName();
            string IPAddress = Dns.GetHostEntry(hostName).AddressList[0].ToString();

            if (!IsSuperStarIgnited)
                await IgniteSuperStarAsync();

            OASISResult<IAvatar> result = await OASISAPI.Avatar.AuthenticateAsync(username, password, IPAddress);

            if (!result.IsError)
                LoggedInUser = (Avatar)result.Result;

            return result;
        }

        public static OASISResult<IAvatar> CreateAvatar(string title, string firstName, string lastName, string username, string password, ConsoleColor cliColour = ConsoleColor.Green, ConsoleColor favColour = ConsoleColor.Green)
        {
            if (!IsSuperStarIgnited)
                IgniteSuperStar();

            return OASISAPI.Avatar.Register(title, firstName, lastName, username, password, AvatarType.User, "https://api.oasisplatform.world/api", OASISType.STARCLI, cliColour, favColour);
        }

        public static async Task<OASISResult<IAvatar>> CreateAvatarAsync(string title, string firstName, string lastName, string username, string password, ConsoleColor cliColour = ConsoleColor.Green, ConsoleColor favColour = ConsoleColor.Green)
        {
            if (!IsSuperStarIgnited)
                await IgniteSuperStarAsync();

            //TODO: Implement Async version of Register and call instead of below:
            return OASISAPI.Avatar.Register(title, firstName, lastName, username, password, AvatarType.User, "https://api.oasisplatform.world/api", OASISType.STARCLI, cliColour, favColour);
        }

        public static OASISResult<IAvatar> BeamIn(string username, string password)
        {
            string hostName = Dns.GetHostName();
            string IPAddress = Dns.GetHostEntry(hostName).AddressList[2].ToString();
            //string IPAddress = Dns.GetHostByName(hostName).AddressList[3].ToString();
            //+string IPAddress = Dns.GetHostByName(hostName).AddressList[4].ToString();

            if (!IsSuperStarIgnited)
                IgniteSuperStar();

            OASISResult<IAvatar> result = OASISAPI.Avatar.Authenticate(username, password, IPAddress);

            if (!result.IsError)
                LoggedInUser = (Avatar)result.Result;

            return result;
        }

        public static async Task<CoronalEjection> LightAsync(GenesisType type, string name, string dnaFolder = "", string genesisCSharpFolder = "", string genesisRustFolder = "", string genesisNameSpace = "")
        {
            return await LightAsync(type, name, (ICelestialBody)null, dnaFolder, genesisCSharpFolder, genesisRustFolder, genesisNameSpace);
        }

        public static async Task<CoronalEjection> LightAsync(GenesisType type, string name, IStar starToAddPlanetTo = null, string dnaFolder = "", string genesisCSharpFolder = "", string genesisRustFolder = "", string genesisNameSpace = "")
        {
            return await LightAsync(type, name, (ICelestialBody)starToAddPlanetTo, dnaFolder, genesisCSharpFolder, genesisRustFolder, genesisNameSpace);
        }

        public static async Task<CoronalEjection> LightAsync(GenesisType type, string name, IPlanet planetToAddMoonTo = null, string dnaFolder = "", string genesisCSharpFolder = "", string genesisRustFolder = "", string genesisNameSpace = "")
        {
            return await LightAsync(type, name, (ICelestialBody)planetToAddMoonTo, dnaFolder, genesisCSharpFolder, genesisRustFolder, genesisNameSpace);
        }

        //TODO: Create non async version of Light();
        private static async Task<CoronalEjection> LightAsync(GenesisType type, string name, ICelestialBody celestialBodyParent = null, string dnaFolder = "", string genesisCSharpFolder = "", string genesisRustFolder = "", string genesisNameSpace = "")
        {
            CelestialBody newBody = null;
            bool holonReached = false;
            string holonBufferRust = "";
            string holonBufferCsharp = "";
            string libBuffer = "";
            string holonName = "";
            string zomeName = "";
            string holonFieldsClone = "";
            int nextLineToWrite = 0;
            bool firstField = true;
            string iholonBuffer = "";
            string zomeBufferCsharp = "";
            string celestialBodyBufferCsharp = "";
            bool firstHolon = true;

            if (LoggedInUser == null)
                return new CoronalEjection() { ErrorOccured = true, Message = "Avatar is not logged in. Please log in before calling this command." };

            if (LoggedInUser.Level < 33 && type == GenesisType.Planet)
                return new CoronalEjection() { ErrorOccured = true, Message = "Avatar must have reached level 33 before they can create planets. Please create a moon instead..." };

            if (celestialBodyParent == null && type == GenesisType.Moon)
                return new CoronalEjection() { ErrorOccured = true, Message = "You must specify the planet to add the moon to." };

            if (!IsSuperStarIgnited)
                await IgniteSuperStarAsync();

            ValidateLightDNA(dnaFolder, genesisCSharpFolder, genesisRustFolder);

            string rustDNAFolder = string.Empty;

            switch (STARDNA.HolochainVersion.ToUpper())
            {
                case "REDUX":
                    rustDNAFolder = STARDNA.RustDNAReduxTemplateFolder;
                    break;

                case "RSM":
                    rustDNAFolder = STARDNA.RustDNARSMTemplateFolder;
                    break;
            }

            string libTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateLib)).OpenText().ReadToEnd();
            string createTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateCreate)).OpenText().ReadToEnd();
            string readTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateRead)).OpenText().ReadToEnd();
            string updateTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateUpdate)).OpenText().ReadToEnd();
            string deleteTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateDelete)).OpenText().ReadToEnd();
            string listTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateList)).OpenText().ReadToEnd();
            string validationTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateValidation)).OpenText().ReadToEnd();
            string holonTemplateRust = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateHolon)).OpenText().ReadToEnd();
            string intTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateInt)).OpenText().ReadToEnd();
            string stringTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateString)).OpenText().ReadToEnd();
            string boolTemplate = new FileInfo(string.Concat(rustDNAFolder, "\\", STARDNA.RustTemplateBool)).OpenText().ReadToEnd();

            string iHolonTemplate = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateIHolonDNA)).OpenText().ReadToEnd();
            string holonTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateHolonDNA)).OpenText().ReadToEnd();
            string zomeTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateZomeDNA)).OpenText().ReadToEnd();
            string iStarTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateIStarDNA)).OpenText().ReadToEnd();
            string starTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateStarDNA)).OpenText().ReadToEnd();
            string iPlanetTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateIPlanetDNA)).OpenText().ReadToEnd();
            string planetTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplatePlanetDNA)).OpenText().ReadToEnd();
            string iMoonTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateIMoonDNA)).OpenText().ReadToEnd();
            string moonTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateMoonDNA)).OpenText().ReadToEnd();
            string TemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplatePlanetDNA)).OpenText().ReadToEnd();

            string iCelestialBodyTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateICelestialBodyDNA)).OpenText().ReadToEnd();
            string celestialBodyTemplateCsharp = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateCelestialBodyDNA)).OpenText().ReadToEnd();
            string iZomeTemplate = new FileInfo(string.Concat(STARDNA.CSharpDNATemplateFolder, "\\", STARDNA.CSharpTemplateIZomeDNA)).OpenText().ReadToEnd();

            //If folder is not passed in via command line args then use default in config file.
            if (string.IsNullOrEmpty(dnaFolder))
                dnaFolder = STARDNA.CelestialBodyDNA;

            if (string.IsNullOrEmpty(genesisCSharpFolder))
                genesisCSharpFolder = STARDNA.GenesisCSharpFolder;

            if (string.IsNullOrEmpty(genesisRustFolder))
                genesisRustFolder = STARDNA.GenesisRustFolder;

            if (string.IsNullOrEmpty(genesisNameSpace))
                genesisNameSpace = STARDNA.GenesisNamespace;

            DirectoryInfo dirInfo = new DirectoryInfo(dnaFolder);
            FileInfo[] files = dirInfo.GetFiles();

            switch (type)
            {
                case GenesisType.Moon:
                    {
                        newBody = new Moon();
                        newBody.ParentHolon = celestialBodyParent;
                        newBody.ParentHolonId = celestialBodyParent.Id;
                        newBody.ParentPlanet = (IPlanet)celestialBodyParent;
                        newBody.ParentPlanetId = celestialBodyParent.ParentPlanetId;
                        newBody.ParentStar = celestialBodyParent.ParentStar;
                        newBody.ParentStarId = celestialBodyParent.ParentStarId;
                    }
                    break;

                case GenesisType.Planet:
                    {
                        newBody = new Planet();

                        //If new parent Star is passed in then set the parent star to SuperStar.
                        if (celestialBodyParent == null)
                            celestialBodyParent = InnerStar;

                        newBody.ParentHolon = celestialBodyParent;
                        newBody.ParentStar = InnerStar;
                        newBody.ParentHolon = celestialBodyParent;
                        newBody.ParentHolonId = celestialBodyParent.Id;
                        newBody.ParentStar = celestialBodyParent.ParentStar;
                        newBody.ParentStarId = celestialBodyParent.ParentStarId;
                    }
                break;

                case GenesisType.Star:
                    {
                        newBody = new Star();
                        newBody.ParentHolon = InnerStar;
                        newBody.ParentHolonId = InnerStar.Id;
                        newBody.ParentStar = InnerStar;
                        newBody.ParentStarId = celestialBodyParent.ParentStarId;
                    }
                break;
            }

           // newBody.CelestialBody = newBody; //TODO: Causes an infinite recursion because CelestialBody is a Holon itself so its linking to itself.
            newBody.Name = name;
            newBody.OnZomeError += NewBody_OnZomeError;
            await newBody.Initialize();
            OASISResult<ICelestialBody> newBodyResult = await newBody.SaveAsync(); //Need to save to get the id to be used for ParentId below (zomes, holons & nodes).

            if (newBodyResult.IsError)
            {
                //TODO: Handle error here.
            }
            else
                newBody = (CelestialBody)newBodyResult.Result;

            //TODO: MOVE ALL RUST CODE INTO HOLOOASIS.GENERATENATIVECODE METHOD.
            IZome currentZome = null;
            IHolon currentHolon = null;

            foreach (FileInfo file in files)
            {
                if (file != null)
                {
                    StreamReader reader = file.OpenText();

                    while (!reader.EndOfStream)
                    {
                        string buffer = reader.ReadLine();

                        if (buffer.Contains("namespace"))
                        {
                            string[] parts = buffer.Split(' ');

                            //If the new namespace name has not been passed in then default it to the proxy holon namespace.
                            if (string.IsNullOrEmpty(genesisNameSpace))
                                genesisNameSpace = parts[1];

                            zomeBufferCsharp = zomeTemplateCsharp.Replace(STARDNA.TemplateNamespace, genesisNameSpace);
                            holonBufferCsharp = holonTemplateCsharp.Replace(STARDNA.TemplateNamespace, genesisNameSpace);
                        }

                        if (buffer.Contains("ZomeDNA"))
                        {
                            string[] parts = buffer.Split(' ');
                            libBuffer = libTemplate.Replace("zome_name", parts[6].ToSnakeCase());

                            zomeBufferCsharp = zomeBufferCsharp.Replace("ZomeDNATemplate", parts[6].ToPascalCase());
                            zomeBufferCsharp = zomeBufferCsharp.Replace("{zome}", parts[6].ToSnakeCase());
                            zomeName = parts[6].ToPascalCase();

                            currentZome = new Zome()
                            {
                                Name = zomeName,
                                HolonType = HolonType.Zome,
                                ParentHolonId = newBody.Id,
                                ParentPlanetId = newBody.HolonType == HolonType.Planet ? newBody.Id : Guid.Empty,
                                ParentMoonId = newBody.HolonType == HolonType.Moon ? newBody.Id : Guid.Empty
                            };

                            //currentZome = new Zome() { Name = zomeName, HolonType = HolonType.Zome, ParentId = newBody.Id, ParentCelestialBodyId = newBody.Id };
                            await newBody.CelestialBodyCore.AddZome(currentZome); //TODO: May need to save this once holons and nodes/fields have been added?
                        }

                        if (holonReached && buffer.Contains("string") || buffer.Contains("int") || buffer.Contains("bool"))
                        {
                            string[] parts = buffer.Split(' ');
                            string fieldName = parts[14].ToSnakeCase();

                            switch (parts[13].ToLower())
                            {
                                case "string":
                                    GenerateRustField(fieldName, stringTemplate, NodeType.String, holonName, currentHolon, ref firstField, ref holonFieldsClone, ref holonBufferRust);
                                    break;

                                case "int":
                                    GenerateRustField(fieldName, intTemplate, NodeType.Int, holonName, currentHolon, ref firstField, ref holonFieldsClone, ref holonBufferRust);
                                    break;

                                case "bool":
                                    GenerateRustField(fieldName, boolTemplate, NodeType.Bool, holonName, currentHolon, ref firstField, ref holonFieldsClone, ref holonBufferRust);
                                    break;
                            }
                        }

                        // Write the holon out to the rust lib template. 
                        if (holonReached && buffer.Length > 1 && buffer.Substring(buffer.Length - 1, 1) == "}" && !buffer.Contains("get;"))
                        {
                            if (holonBufferRust.Length > 2)
                                holonBufferRust = holonBufferRust.Remove(holonBufferRust.Length - 3);

                            holonBufferRust = string.Concat(Environment.NewLine, holonBufferRust, Environment.NewLine, holonTemplateRust.Substring(holonTemplateRust.Length - 1, 1), Environment.NewLine);

                            int zomeIndex = libTemplate.IndexOf("#[zome]");
                            int zomeBodyStartIndex = libTemplate.IndexOf("{", zomeIndex);

                            libBuffer = libBuffer.Insert(zomeIndex - 2, holonBufferRust);

                            if (nextLineToWrite == 0)
                                nextLineToWrite = zomeBodyStartIndex + holonBufferRust.Length;
                            else
                                nextLineToWrite += holonBufferRust.Length;

                            //Now insert the CRUD methods for each holon.
                            libBuffer = libBuffer.Insert(nextLineToWrite + 2, string.Concat(Environment.NewLine, createTemplate.Replace("Holon", holonName.ToPascalCase()).Replace("{holon}", holonName), Environment.NewLine));
                            libBuffer = libBuffer.Insert(nextLineToWrite + 2, string.Concat(Environment.NewLine, readTemplate.Replace("Holon", holonName.ToPascalCase()).Replace("{holon}", holonName), Environment.NewLine));
                            libBuffer = libBuffer.Insert(nextLineToWrite + 2, string.Concat(Environment.NewLine, updateTemplate.Replace("Holon", holonName.ToPascalCase()).Replace("{holon}", holonName).Replace("//#CopyFields//", holonFieldsClone), Environment.NewLine));
                            libBuffer = libBuffer.Insert(nextLineToWrite + 2, string.Concat(Environment.NewLine, deleteTemplate.Replace("Holon", holonName.ToPascalCase()).Replace("{holon}", holonName), Environment.NewLine));
                            libBuffer = libBuffer.Insert(nextLineToWrite + 2, string.Concat(Environment.NewLine, validationTemplate.Replace("Holon", holonName.ToPascalCase()).Replace("{holon}", holonName), Environment.NewLine));

                            if (!firstHolon)
                            {
                                //TODO: Need to make dynamic so no need to pass length in (had issues before but will try again later... :) )
                                zomeBufferCsharp = GenerateDynamicZomeFunc("Load", zomeTemplateCsharp, holonName, zomeBufferCsharp, 170);
                                zomeBufferCsharp = GenerateDynamicZomeFunc("Save", zomeTemplateCsharp, holonName, zomeBufferCsharp, 147);
                            }

                            holonName = holonName.ToPascalCase();

                            File.WriteAllText(string.Concat(genesisCSharpFolder, "\\I", holonName, ".cs"), iholonBuffer);
                            File.WriteAllText(string.Concat(genesisCSharpFolder, "\\", holonName, ".cs"), holonBufferCsharp);

                            //TDOD: Finish putting in IZomeBuffer etc
                            //   File.WriteAllText(string.Concat(genesisCSharpFolder, "\\I", holonName, ".cs"), izomeBuffer);
                            // File.WriteAllText(string.Concat(genesisCSharpFolder, "\\", zomeName, ".cs"), zomeBufferCsharp);

                            holonBufferRust = "";
                            holonBufferCsharp = "";
                            holonFieldsClone = "";
                            holonReached = false;
                            firstField = true;
                            firstHolon = false;
                            holonName = "";
                        }

                        if (buffer.Contains("HolonDNA"))
                        {
                            string[] parts = buffer.Split(' ');
                            holonName = parts[10].ToPascalCase();

                            holonBufferRust = holonTemplateRust.Replace("Holon", holonName).Replace("{holon}", holonName.ToSnakeCase());
                            holonBufferRust = holonBufferRust.Substring(0, holonBufferRust.Length - 1);

                            //Process the CSharp Templates.
                            if (string.IsNullOrEmpty(holonBufferCsharp))
                                holonBufferCsharp = holonTemplateCsharp;

                            holonBufferCsharp = holonBufferCsharp.Replace("HolonDNATemplate", parts[10]);
                            iholonBuffer = iHolonTemplate.Replace("IHolonDNATemplate", string.Concat("I", parts[10]));

                            zomeBufferCsharp = zomeBufferCsharp.Replace("HOLON", parts[10].ToPascalCase());
                            zomeBufferCsharp = zomeBufferCsharp.Replace("{holon}", parts[10].ToSnakeCase());

                            zomeBufferCsharp = zomeBufferCsharp.Replace(STARDNA.TemplateNamespace, genesisNameSpace);
                            holonBufferCsharp = holonBufferCsharp.Replace(STARDNA.TemplateNamespace, genesisNameSpace);
                            iholonBuffer = iholonBuffer.Replace(STARDNA.TemplateNamespace, genesisNameSpace);

                            if (string.IsNullOrEmpty(celestialBodyBufferCsharp))
                                celestialBodyBufferCsharp = celestialBodyTemplateCsharp;

                            celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace(STARDNA.TemplateNamespace, genesisNameSpace);
                            celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace("CelestialBodyDNATemplate", name.ToPascalCase());
                            celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace("{holon}", parts[10].ToSnakeCase()).Replace("HOLON", parts[10].ToPascalCase());
                            celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace("CelestialBody", Enum.GetName(typeof(GenesisType), type)).Replace("ICelestialBody", string.Concat("I", Enum.GetName(typeof(GenesisType), type)));
                            celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace("ICelestialBody", string.Concat("I", Enum.GetName(typeof(GenesisType), type)));
                            //celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace("GenesisType.Star", string.Concat("GenesisType.", Enum.GetName(typeof(GenesisType), type)));
                            celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace(", GenesisType.Star", "");

                            /*
                            switch (type)
                            {
                                case GenesisType.Star:
                                    celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace("CelestialBody", "Star").Replace("ICelestialBody", "IStar");
                                    break;

                                case GenesisType.Planet:
                                    celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace("CelestialBody", "Planet").Replace("ICelestialBody", "IPlanet");
                                    break;

                                case GenesisType.Moon:
                                    celestialBodyBufferCsharp = celestialBodyBufferCsharp.Replace("CelestialBody", "Moon").Replace("ICelestialBody", "IMoon");
                                    break;
                            }*/

                            // TODO: Current Zome Id will be empty here so need to save the zome before? (above when the zome is first created and added to the newBody zomes collection).
                            currentHolon = new Holon() 
                            { 
                                Name = holonName, 
                                HolonType = HolonType.Holon, 
                                ParentHolonId = currentZome.Id, 
                                ParentZomeId = currentZome.Id, 
                                ParentPlanetId = newBody.HolonType == HolonType.Planet ? newBody.Id : Guid.Empty, 
                                ParentMoonId = newBody.HolonType == HolonType.Moon ? newBody.Id : Guid.Empty 
                            };

                            currentZome.Holons.Add((Holon)currentHolon); 

                            holonName = holonName.ToSnakeCase();
                            holonReached = true;
                        }
                    }

                    reader.Close();
                    nextLineToWrite = 0;

                    File.WriteAllText(string.Concat(genesisRustFolder, "\\lib.rs"), libBuffer);
                    File.WriteAllText(string.Concat(genesisCSharpFolder, "\\", zomeName, ".cs"), zomeBufferCsharp);
                }
            }

            // Remove any white space from the name.
            File.WriteAllText(string.Concat(genesisCSharpFolder, "\\", Regex.Replace(name, @"\s+", ""), Enum.GetName(typeof(GenesisType), type), ".cs"), celestialBodyBufferCsharp);

          //  if (currentZome != null)
           //     newBody.CelestialBodyCore.Zomes.Add(currentZome);

            //TODO: Need to save the collection of Zomes/Holons that belong to this planet here...
            await newBody.SaveAsync(); // Need to save again so newly added zomes/holons/nodes are also saved.

            switch (type)
            {
                case GenesisType.Moon:
                    {
                        await ((PlanetCore)celestialBodyParent.CelestialBodyCore).AddMoonAsync((IMoon)newBody);
                        return new CoronalEjection() { ErrorOccured = false, Message = "Moon Successfully Created.", CelestialBody = newBody };
                    }

                case GenesisType.Planet:
                    {
                        // If a star is not passed in, then add the planet to the main star.
                        if (celestialBodyParent == null)
                            celestialBodyParent = InnerStar;

                        OASISResult<IPlanet> result = await ((StarCore)celestialBodyParent.CelestialBodyCore).AddPlanetAsync((IPlanet)newBody);

                        if (result != null)
                        { 
                            if (result.IsError)
                                return new CoronalEjection() { ErrorOccured = true, Message = result.Message, CelestialBody = result.Result };
                            else
                                return new CoronalEjection() { ErrorOccured = false, Message = "Planet Successfully Created.", CelestialBody = result.Result };
                        }
                        else
                            return new CoronalEjection() { ErrorOccured = true, Message = "Unknown Error Occured Creating Planet." };
                    }

                case GenesisType.Star:
                    {
                        await SuperStarCore.AddStarAsync((IStar)newBody);
                        return new CoronalEjection() { ErrorOccured = false, Message = "Star Successfully Created.", CelestialBody = newBody };
                    }
                default:
                    return new CoronalEjection() { ErrorOccured = true, Message = "Unknown Error Occured.", CelestialBody = newBody };
            }


            //Generate any native code for the current provider.
            //TODO: Add option to pass into STAR which providers to generate native code for (can be more than one provider).
            ((IOASISSuperStar)ProviderManager.CurrentStorageProvider).NativeCodeGenesis(newBody);

            //TODO: Need to save this to the StarNET store (still to be made!) (Will of course be written on top of the HDK/ODK...
            //This will be private on the store until the user publishes via the Star.Seed() command.
        }

        // Build
        public static CoronalEjection Flare(string bodyName)
        {
            //TODO: Build rust code using hc conductor and .net code using dotnet compiler.
            return new CoronalEjection();
        }

        public static CoronalEjection Flare(CelestialBody body)
        {
            //TODO: Build rust code using hc conductor and .net code using dotnet compiler.
            return new CoronalEjection();
        }

        //Activate & Launch - Launch & activate a planet (OAPP) by shining the star's light upon it...
        public static void Shine(CelestialBody body)
        {

        }

        public static void Shine(string bodyName)
        {

        }

        //Dractivate
        public static void Dim(CelestialBody body)
        {

        }

        public static void Dim(string bodyName)
        {

        }

        //Deploy
        public static void Seed(CelestialBody body)
        {

        }

        public static void Seed(string bodyName)
        {

        }

        // Run Tests
        public static void Twinkle(CelestialBody body)
        {

        }

        public static void Twinkle(string bodyName)
        {

        }

        // Delete Planet (OAPP)
        public static void Dust(CelestialBody body)
        {

        }

        public static void Dust(string bodyName)
        {

        }

        // Delete Planet (OAPP)
        public static void Evolve(CelestialBody body)
        {

        }

        public static void Evolve(string bodyName)
        {

        }

        // Delete Planet (OAPP)
        public static void Mutate(CelestialBody body)
        {

        }

        public static void Mutate(string bodyName)
        {

        }

        // Highlight the Planet (OAPP) in the OAPP Store (StarNET)
        public static void Radiate(CelestialBody body)
        {

        }

        public static void Radiate(string bodyName)
        {

        }

        // Show how much light the planet (OAPP) is emitting into the solar system (StarNET/HoloNET)
        public static void Emit(CelestialBody body)
        {

        }

        public static void Emit(string bodyName)
        {

        }

        // Show stats of the Planet (OAPP)
        public static void Reflect(CelestialBody body)
        {

        }

        public static void Reflect(string bodyName)
        {

        }

        // Send/Receive Love
        public static void Love(CelestialBody body)
        {

        }

        public static void Love(string body)
        {

        }

        // Show network stats/management/settings
        public static void Burst(CelestialBody body)
        {

        }

        public static void Burst(string body)
        {

        }

        // ????
        public static void Pulse(CelestialBody body)
        {

        }

        public static void Pulse(string body)
        {

        }

        // Reserved For Future Use...
        public static void Super(CelestialBody body)
        {

        }

        public static void Super(string planetName)
        {

        }

        private static void ValidateSTARDNA(STARDNA starDNA)
        {
            if (starDNA != null)
            {
                ValidateFolder(starDNA.CelestialBodyDNA, "starDNA.CelestialBodyDNA", true);
                ValidateFolder(starDNA.GenesisCSharpFolder, "starDNA.GenesisCSharpFolder", false, true);
                ValidateFolder(starDNA.GenesisRustFolder, "starDNA.GenesisRustFolder", false, true);
                ValidateFolder(starDNA.CSharpDNATemplateFolder, "starDNA.CSharpDNATemplateFolder");
                ValidateFile(starDNA.CSharpDNATemplateFolder, starDNA.CSharpTemplateHolonDNA, "starDNA.CSharpTemplateHolonDNA");
                ValidateFile(starDNA.CSharpDNATemplateFolder, starDNA.CSharpTemplateZomeDNA, "starDNA.CSharpTemplateZomeDNA");
                ValidateFile(starDNA.CSharpDNATemplateFolder, starDNA.CSharpTemplateIStarDNA, "starDNA.CSharpTemplateIStarDNA");
                ValidateFile(starDNA.CSharpDNATemplateFolder, starDNA.CSharpTemplateStarDNA, "starDNA.CSharpTemplateStarDNA");
                ValidateFile(starDNA.CSharpDNATemplateFolder, starDNA.CSharpTemplateIPlanetDNA, "starDNA.CSharpTemplateIPlanetDNA");
                ValidateFile(starDNA.CSharpDNATemplateFolder, starDNA.CSharpTemplateIPlanetDNA, "starDNA.CSharpTemplatePlanetDNA");
                ValidateFile(starDNA.CSharpDNATemplateFolder, starDNA.CSharpTemplateIMoonDNA, "starDNA.CSharpTemplateIMoonDNA");
                ValidateFile(starDNA.CSharpDNATemplateFolder, starDNA.CSharpTemplateIMoonDNA, "starDNA.CSharpTemplateMoonDNA");
                ValidateFile(starDNA.CSharpDNATemplateFolder, starDNA.CSharpTemplateCelestialBodyDNA, "starDNA.CSharpTemplateCelestialBodyDNA");

                switch (starDNA.HolochainVersion.ToUpper())
                {
                    case "REDUX":
                        {
                            ValidateFolder(starDNA.RustDNAReduxTemplateFolder, "starDNA.RustDNAReduxTemplateFolder");
                            ValidateFile(starDNA.RustDNAReduxTemplateFolder, starDNA.RustTemplateCreate, "starDNA.RustTemplateCreate");
                            ValidateFile(starDNA.RustDNAReduxTemplateFolder, starDNA.RustTemplateDelete, "starDNA.RustTemplateDelete");
                            ValidateFile(starDNA.RustDNAReduxTemplateFolder, starDNA.RustTemplateLib, "starDNA.RustTemplateLib");
                            ValidateFile(starDNA.RustDNAReduxTemplateFolder, starDNA.RustTemplateRead, "starDNA.RustTemplateRead");
                            ValidateFile(starDNA.RustDNAReduxTemplateFolder, starDNA.RustTemplateUpdate, "starDNA.RustTemplateUpdate");
                            ValidateFile(starDNA.RustDNAReduxTemplateFolder, starDNA.RustTemplateList, "starDNA.RustTemplateList");
                            ValidateFile(starDNA.RustDNAReduxTemplateFolder, starDNA.RustTemplateValidation, "starDNA.RustTemplateValidation");
                        }
                        break;

                    case "RSM":
                        {
                            ValidateFolder(starDNA.RustDNARSMTemplateFolder, "starDNA.RustDNARSMTemplateFolder");
                            ValidateFile(starDNA.RustDNARSMTemplateFolder, starDNA.RustTemplateCreate, "starDNA.RustTemplateCreate");
                            ValidateFile(starDNA.RustDNARSMTemplateFolder, starDNA.RustTemplateDelete, "starDNA.RustTemplateDelete");
                            ValidateFile(starDNA.RustDNARSMTemplateFolder, starDNA.RustTemplateLib, "starDNA.RustTemplateLib");
                            ValidateFile(starDNA.RustDNARSMTemplateFolder, starDNA.RustTemplateRead, "starDNA.RustTemplateRead");
                            ValidateFile(starDNA.RustDNARSMTemplateFolder, starDNA.RustTemplateUpdate, "starDNA.RustTemplateUpdate");
                            ValidateFile(starDNA.RustDNARSMTemplateFolder, starDNA.RustTemplateList, "starDNA.RustTemplateList");
                            ValidateFile(starDNA.RustDNARSMTemplateFolder, starDNA.RustTemplateValidation, "starDNA.RustTemplateValidation");
                        }
                        break;
                }
            }
            else
                throw new ArgumentNullException("starDNA is null, please check and try again.");
        }

        private static void ValidateLightDNA(string dnaFolder, string genesisCSharpFolder, string genesisRustFolder)
        {
            ValidateFolder(dnaFolder, "dnaFolder");
            ValidateFolder(genesisCSharpFolder, "genesisCSharpFolder", false, true);
            ValidateFolder(genesisRustFolder, "genesisRustFolder", false, true);
        }

        private static void ValidateFolder(string folder, string folderParam, bool checkIfContainsFiles = false, bool createIfDoesNotExist = false)
        {
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentNullException(folderParam, string.Concat("The ", folderParam, " param in the StarDNA is null, please double check and try again."));

            if (checkIfContainsFiles && Directory.GetFiles(folder).Length == 0)
                throw new InvalidOperationException(string.Concat("The ", folderParam, " folder in the StarDNA is empty."));

            if (!Directory.Exists(folder))
            {
                if (createIfDoesNotExist)
                    Directory.CreateDirectory(folder);
                else
                    throw new InvalidOperationException(string.Concat("The ", folderParam, " was not found, please double check and try again."));
            }
        }

        private static void ValidateFile(string folder, string file, string fileParam)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(fileParam, string.Concat("The ", fileParam, " param in the StarDNA is null, please double check and try again."));

            if (!File.Exists(string.Concat(folder, "\\", file)))
                throw new FileNotFoundException(string.Concat("The ", fileParam, " file is not valid, please double check and try again."), string.Concat(folder, "\\", file));
        }

        private static STARDNA LoadDNA()
        {
            using (StreamReader r = new StreamReader(STARDNAPath))
            {
                string json = r.ReadToEnd();
                STARDNA = JsonConvert.DeserializeObject<STARDNA> (json);
                return STARDNA;
            }
        }
        private static bool SaveDNA()
        {
            string json = JsonConvert.SerializeObject(STARDNA);
            StreamWriter writer = new StreamWriter(STARDNAPath);
            writer.Write(json);
            writer.Close();
            
            return true;
        }

        private static void NewBody_OnZomeError(object sender, ZomeErrorEventArgs e)
        {
            //OnZomeError?.Invoke(sender, new ZomeErrorEventArgs() { EndPoint = StarBody.HoloNETClient.EndPoint, Reason = e.Reason, ErrorDetails = e.ErrorDetails, HoloNETErrorDetails = e.HoloNETErrorDetails });
            // OnStarError?.Invoke(sender, new StarErrorEventArgs() { EndPoint = StarBody.HoloNETClient.EndPoint, Reason = e.Reason, ErrorDetails = e.ErrorDetails, HoloNETErrorDetails = e.HoloNETErrorDetails });
        }

        //TODO: Get this working... :) // Is this working now?! lol hmmmm... need to check...
        private static string GenerateDynamicZomeFunc(string funcName, string zomeTemplateCsharp, string holonName, string zomeBufferCsharp, int funcLength)
        {
            int funcHolonIndex = zomeTemplateCsharp.IndexOf(funcName);
            string funct = zomeTemplateCsharp.Substring(funcHolonIndex - 26, funcLength); //170
            funct = funct.Replace("{holon}", holonName.ToSnakeCase()).Replace("HOLON", holonName.ToPascalCase());
            zomeBufferCsharp = zomeBufferCsharp.Insert(zomeBufferCsharp.Length - 6, funct);
            return zomeBufferCsharp;
        }

        private static void GenerateRustField(string fieldName, string fieldTemplate, NodeType nodeType, string holonName, IHolon currentHolon, ref bool firstField, ref string holonFieldsClone, ref string holonBufferRust)
        {
            if (firstField)
                firstField = false;
            else
                holonFieldsClone = string.Concat(holonFieldsClone, "\t");

            holonFieldsClone = string.Concat(holonFieldsClone, holonName, ".", fieldName, "=updated_entry.", fieldName, ";", Environment.NewLine);
            holonBufferRust = string.Concat(holonBufferRust, fieldTemplate.Replace("variableName", fieldName), ",", Environment.NewLine);

            if (currentHolon.Nodes == null)
                currentHolon.Nodes = new List<INode>();

            //currentHolon.Nodes.Add(new Node { NodeName = fieldName.ToPascalCase(), NodeType = nodeType, Parent = currentHolon, ParentId = currentHolon.Id });
            currentHolon.Nodes.Add(new Node { NodeName = fieldName.ToPascalCase(), NodeType = nodeType, ParentId = currentHolon.Id });
        }

        //private static OASISResult<T> HandleError<T>(ref OASISResult<T> result, string errorMessage)
        //{
        //    result.IsError = true;
        //    result.ErrorMessage = errorMessage;
        //    LoggingManager.Log(errorMessage, API.Core.Enums.LogType.Error);
        //    return result;
        //}

        //private static OASISResult<T> HandleWarning<T>(ref OASISResult<T> result, string message)
        //{
        //    result.IsWarning = true;
        //    result.Message = message;
        //    LoggingManager.Log(message, API.Core.Enums.LogType.Warn);
        //    return result;
        //}

        private static OASISResult<IOASISStorage> InitializeOASISAPI(ref OASISResult<ICelestialBody> result, InitOptions OASISAPIInitOptions, string STARDNAPath = STAR_DNA_DEFAULT_PATH, string OASISDNAPath = OASIS_DNA_DEFAULT_PATH)
        {
            SuperStar.OASISDNAPath = OASISDNAPath;

            //By default the OASISDNAManager will load the settings from OASIS_DNA.json in the current working dir but you can override using below:
            OASISDNAManager.OASISDNAFileName = SuperStar.OASISDNAPath;
            return OASISDNAManager.GetAndActivateDefaultProvider(); 

            /*
            // TODO: Not sure if we want to store the OASIS Providers in the STARDNA? Maybe better to just use those listed in OASISDNA? Think it may be, lol! ;-)
            if (string.IsNullOrEmpty(STARDNA.OASISProviders))
            {
                // Will initialize the default OASIS Provider defined OASIS_DNA config file.
                OASISAPI.Init(OASISAPIInitOptions, OASISDNAManager.OASISDNA);
                HandleWarning(ref result, "No providers were found in the OASISProviders list in the STARDNA file so initializing the OASIS API using defaults in the OASISDNA file.");
            }
            else
            {
                string[] parts = STARDNA.OASISProviders.Split(',');
                List<ProviderType> providerTypes = new List<ProviderType>();

                object providerTypeObject = null;
                foreach (string part in parts)
                {
                    if (Enum.TryParse(typeof(ProviderType), part, out providerTypeObject))
                        providerTypes.Add((ProviderType)providerTypeObject);
                    else
                    {
                        HandleError(ref result, string.Concat("The provider type ", part, " specified in the OASISProviders list in the STARDNA file is not valid. Valid providers are: ", EnumHelper.GetEnumValues(typeof(ProviderType), EnumHelper.ListType.ItemsSeperatedByComma)));
                        return;
                    }
                }

                OASISResult<bool> oasisAPIResult = OASISAPI.Init(providerTypes, OASISDNAManager.OASISDNA);

                if (oasisAPIResult.IsError)
                    HandleError(ref result, string.Concat("Error initialiazing OASIS API. Reason: ", oasisAPIResult.ErrorMessage));
            }*/
        }

        private static void IgniteInnerStar(ref OASISResult<ICelestialBody> result, string starId = null)
        {
            Guid starIdGuid = Guid.Empty;

            // If the starId is passed in and is valid then convert to Guid, otherwise get it from the STARDNA file.
            if (!string.IsNullOrEmpty(starId) && !string.IsNullOrWhiteSpace(starId))
            {
                if (!Guid.TryParse(starId, out starIdGuid))
                {
                    //TODO: Need to apply this error handling across the entire OASIS eventually...
                    ErrorHandling.HandleError(ref result, "StarID passed in is invalid. It needs to be a valid Guid.");
                    return;
                }
            }
            else
            {
                if (!Guid.TryParse(STARDNA.StarId, out starIdGuid))
                {
                    ErrorHandling.HandleError(ref result, "StarID defined in the STARDNA file in is invalid. It needs to be a valid Guid.");
                    return;
                }
            }

            InnerStar = new Star(starIdGuid);

            if (InnerStar.Id == Guid.Empty)
            {
                // TODO: May possibly have one SuperStar per Provider Type? Or list of ProviderTypes? People can host whichever provider(s) they wish as a ONODE. Each ONODE will be a SuperStar, which can choose which Glaxies/Provider Types to host. Therefore the entire ONET (OASIS Network) is the distributed de-centralised network of SuperStars/Galaxies forming the OASIS Universe or OASIS meta-verse/magicverse. :)
                InnerStar.Name = "SuperStar";
                InnerStar.Description = "SuperStar at the centre of a Galaxy. Can create other stars, planets (Super OAPPS) and moons (OAPPS)";
                InnerStar.HolonType = HolonType.SuperStar;
            }
            else
                result.Message = "SuperSTAR Ignited";
        }

        private static OASISResult<ICelestialBody> IgniteSuperStarInternal(InitOptions OASISAPIInitOptions, string STARDNAPath = STAR_DNA_DEFAULT_PATH, string OASISDNAPath = OASIS_DNA_DEFAULT_PATH, string starId = null)
        {
            OASISResult<ICelestialBody> result = new OASISResult<ICelestialBody>();

            // If you wish to change the logging framework from the default (NLog) then set it below (or just change in OASIS_DNA - prefered way)
            //LoggingManager.CurrentLoggingFramework = LoggingFramework.NLog;

            if (File.Exists(STARDNAPath))
                LoadDNA();
            else
            {
                STARDNA = new STARDNA();
                SaveDNA();
            }

            ValidateSTARDNA(STARDNA);
            OASISResult<IOASISStorage> oasisResult = InitializeOASISAPI(ref result, OASISAPIInitOptions, STARDNAPath, OASISDNAPath);

            if (oasisResult.IsError)
            {
                result.IsError = true;
                result.Message = oasisResult.Message;
                return result;
            }

            if (!result.IsError)
                IgniteInnerStar(ref result, starId);

            if (!result.IsError)
            {
                SuperStarCore = new SuperStarCore(InnerStar.Id);
                WireUpEvents();
            }

            return result;
        }
    }
}
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NextGenSoftware.OASIS.API.DNA;
using NextGenSoftware.OASIS.API.Core.Holons;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Managers;
using NextGenSoftware.OASIS.API.Core.Objects;

namespace NextGenSoftware.OASIS.API.ONODE.WebAPI.Controllers
{


    //[EnableCors(origins: "http://mywebclient.azurewebsites.net", headers: "*", methods: "*")]
    //   [EnableCors()]
    //[Route("api/[mapping]")]
    [Route("api/map")]
    [ApiController]
    public class MapController : OASISControllerBase
    {
        private MapManager _mapManager;

        public MapController()
        {

        }

        private MapManager MapManager
        {
            get
            {
                if (_mapManager == null)
                    _mapManager = new MapManager(GetAndActivateDefaultProvider());

                return _mapManager;
            }
        }

        /// <summary>
        /// Search the map.
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        [HttpGet("Search/{searchParams}")]
        public ActionResult<ISearchResults> Search(ISearchParams searchParams)
        {
            return Ok(MapManager.SearchAsync(searchParams).Result);
        }

        ///// <summary>
        ///// Search the map.
        ///// </summary>
        ///// <param name="searchParams"></param>
        ///// <param name="providerType"></param>
        ///// <param name="setGlobally"></param>
        ///// <returns></returns>
        //[HttpGet("Search/{searchParams}/{providerType}/{setGlobally}")]
        //public ActionResult<ISearchResults> Search(ISearchParams searchParams, ProviderType providerType, bool setGlobally = false)
        //{
        //    return Ok(MapManager.SearchAsync(searchParams).Result);
        //}

        /// <summary>
        /// Create and draw a route on the map between two holons.
        /// </summary>
        /// <param name="holonDNA"></param>
        /// <returns></returns>
        [HttpPost("CreateAndDrawRouteOnMapBetweenHolons/{holonDNA}")]
        public ActionResult<bool> CreateAndDrawRouteOnMapBetweenHolons(HolonDNA holonDNA)
        {
            return MapManager.CreateAndDrawRouteOnMapBetweenHolons(holonDNA.FromHolon, holonDNA.ToHolon);
        }

        ///// <summary>
        ///// Create and draw a route on the map between two holons.
        ///// </summary>
        ///// <param name="holonDNA"></param>
        ///// <returns></returns>
        //[HttpPost("CreateAndDrawRouteOnMapBetweenHolons/{holonDNA}/{providerType}/{setGlobally}")]
        //public ActionResult<bool> CreateAndDrawRouteOnMapBetweenHolons(HolonDNA holonDNA, ProviderType providerType, bool setGlobally = false)
        //{
        //    return MapManager.CreateAndDrawRouteOnMapBetweenHolons(holonDNA.FromHolon, holonDNA.ToHolon);
        //}

        /// <summary>
        /// Create and draw a route on the map between two points.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        [HttpPost("CreateAndDrawRouteOnMapBeweenPoints/{points}")]
        public ActionResult<bool> CreateAndDrawRouteOnMapBeweenPoints(MapPoints points)
        {
            return MapManager.CreateAndDrawRouteOnMapBeweenPoints(points);
        }

        ///// <summary>
        ///// Create and draw a route on the map between two points.
        ///// </summary>
        ///// <param name="points"></param>
        ///// <returns></returns>
        //[HttpPost("CreateAndDrawRouteOnMapBeweenPoints/{points}/{providerType}/{setGlobally}")]
        //public ActionResult<bool> CreateAndDrawRouteOnMapBeweenPoints(MapPoints points, ProviderType providerType, bool setGlobally = false)
        //{
        //    return MapManager.CreateAndDrawRouteOnMapBeweenPoints(points);
        //}

        /// <summary>
        /// Draw a 2D sprint on the Our World HUD.
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [HttpPost("Draw2DSpriteOnHUD/{sprite}/{x}/{y}")]
        //public ActionResult<bool> Draw2DSpriteOnHUD(object sprite, float x, float y)
        public ActionResult<bool> Draw2DSpriteOnHUD(string sprite, float x, float y)
        {
            return MapManager.Draw2DSpriteOnHUD(sprite, x, y);
        }

        ///// <summary>
        ///// Draw a 2D sprint on the Our World HUD.
        ///// </summary>
        ///// <param name="sprite"></param>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <returns></returns>
        //[HttpPost("Draw2DSpriteOnHUD/{sprite}/{x}/{y}/{providerType}/{setGlobally}")]
        ////public ActionResult<bool> Draw2DSpriteOnHUD(object sprite, float x, float y)
        //public ActionResult<bool> Draw2DSpriteOnHUD(string sprite, float x, float y, ProviderType providerType, bool setGlobally = false)
        //{
        //    return MapManager.Draw2DSpriteOnHUD(sprite, x, y);
        //}

        /// <summary>
        /// Draw a 2D sprint on the map.
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [HttpPost("Draw2DSpriteOnMap/{sprite}/{x}/{y}")]
        public ActionResult<bool> Draw2DSpriteOnMap(string sprite, float x, float y)
        {
            return MapManager.Draw2DSpriteOnMap(sprite, x, y);
        }

        ///// <summary>
        ///// Draw a 2D sprint on the map.
        ///// </summary>
        ///// <param name="sprite"></param>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <returns></returns>
        //[HttpPost("Draw2DSpriteOnMap/{sprite}/{x}/{y}")]
        //public ActionResult<bool> Draw2DSpriteOnMap(string sprite, float x, float y, ProviderType providerType, bool setGlobally = false)
        //{
        //    return MapManager.Draw2DSpriteOnMap(sprite, x, y);
        //}

        /// <summary>
        /// Draw a 3D object on the map.
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [HttpPost("Draw3DObjectOnMap/{obj}/{x}/{y}")]
        public ActionResult<bool> Draw3DObjectOnMap(string obj, float x, float y)
        {
            return MapManager.Draw3DObjectOnMap(obj, x, y);
        }

        /// <summary>
        /// Highlight a building on the map.
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        [HttpPost("HighlightBuildingOnMap/{building}")]
        public ActionResult<bool> HighlightBuildingOnMap(Building building)
        {
            return MapManager.HighlightBuildingOnMap(building);
        }

        /// <summary>
        /// Pam the map down.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("PamMapDown/{value}")]
        public ActionResult<bool> PamMapDown(float value)
        {
            return Ok(MapManager.PamMapDown(value));
        }

        /// <summary>
        /// Pam the map left.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("PamMapLeft/{value}")]
        public ActionResult<bool> PamMapLeft(float value)
        {
            return Ok(MapManager.PamMapLeft(value));
        }

        /// <summary>
        /// Pam the map right.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("PamMapRight/{value}")]
        public ActionResult<bool> PamMapRight(float value)
        {
            return Ok(MapManager.PamMapRight(value));
        }

        /// <summary>
        /// Pam the map up.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("PamMapUp/{value}")]
        public ActionResult<bool> PamMapUp(float value)
        {
            return Ok(MapManager.PamMapUp(value));
        }

        /// <summary>
        /// Select building on map.
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        [HttpPost("SelectBuildingOnMap/{building}")]
        public ActionResult<bool> SelectBuildingOnMap(Building building)
        {
            return Ok(MapManager.SelectBuildingOnMap(building));
        }

        /// <summary>
        /// Select holon on map.
        /// </summary>
        /// <param name="holon"></param>
        /// <returns></returns>
        [HttpPost("SelectHolonOnMap/{holon}")]
        public ActionResult<bool> SelectHolonOnMap(Holon holon)
        {
            return Ok(MapManager.SelectHolonOnMap(holon));
        }

        /// <summary>
        /// Select quest on map.
        /// </summary>
        /// <param name="quest"></param>
        /// <returns></returns>
        [HttpPost("SelectQuestOnMap/{quest}")]
        public ActionResult<bool> SelectQuestOnMap(Quest quest)
        {
            return Ok(MapManager.SelectQuestOnMap(quest));
        }

        /// <summary>
        /// Zoom map in.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("ZoomMapIn/{value}")]
        public ActionResult<bool> ZoomMapIn(float value)
        {
            return Ok(MapManager.ZoomMapIn(value));
        }

        /// <summary>
        /// Zoom map out.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("ZoomMapOut/{value}")]
        public ActionResult<bool> ZoomMapOut(float value)
        {
            return Ok(MapManager.ZoomMapOut(value));
        }

        /// <summary>
        /// Zoom to holon on map.
        /// </summary>
        /// <param name="holon"></param>
        /// <returns></returns>
        [HttpPost("ZoomToHolonOnMap/{holon}")]
        public ActionResult<bool> ZoomToHolonOnMap(Holon holon)
        {
            return Ok(MapManager.ZoomToHolonOnMap(holon));
        }

        /// <summary>
        /// Zoom to quest on map.
        /// </summary>
        /// <param name="quest"></param>
        /// <returns></returns>
        [HttpPost("ZoomToQuestOnMap/{quest}")]
        public ActionResult<bool> ZoomToQuestOnMap(Quest quest)
        {
            return Ok(MapManager.ZoomToQuestOnMap(quest));
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Events;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Managers;
using NextGenSoftware.OASIS.API.Core.Objects;

namespace NextGenSoftware.OASIS.API.Core
{
    public abstract class OASISStorageBase : OASISProvider, IOASISStorage
    {
        public event AvatarManager.StorageProviderError StorageProviderError;

        //event StorageProviderError IOASISStorage.StorageProviderError
        //{
        //    add
        //    {
        //        throw new NotImplementedException();
        //    }

        //    remove
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //TODO: COme back to this...
        //public List<Avatar> LoadAvatarsWithoutPasswords(IEnumerable<Avatar> avatars)
        //{
        //    return avatars.Select(x => x.WithoutPassword());
        //}

        //public Avatar WithoutPassword(this Avatar user)
        //{
        //    user.Password = null;
        //    return user;
        //}

        public Task<KarmaAkashicRecord> AddKarmaToAvatarAsync(IAvatarDetail avatar, KarmaTypePositive karmaType, KarmaSourceType karmaSourceType, string karamSourceTitle, string karmaSourceDesc, string karmaSourceWebLink)
        {
            return avatar.KarmaEarntAsync(karmaType, karmaSourceType, karamSourceTitle, karmaSourceDesc, karmaSourceWebLink);
        }

        public Task<KarmaAkashicRecord> RemoveKarmaFromAvatarAsync(IAvatarDetail avatar, KarmaTypeNegative karmaType, KarmaSourceType karmaSourceType, string karamSourceTitle, string karmaSourceDesc, string karmaSourceWebLink)
        {
            return avatar.KarmaLostAsync(karmaType, karmaSourceType, karamSourceTitle, karmaSourceDesc, karmaSourceWebLink);
        }

        public KarmaAkashicRecord AddKarmaToAvatar(IAvatarDetail avatar, KarmaTypePositive karmaType, KarmaSourceType karmaSourceType, string karamSourceTitle, string karmaSourceDesc, string karmaSourceWebLink)
        {
            return avatar.KarmaEarnt(karmaType, karmaSourceType, karamSourceTitle, karmaSourceDesc, karmaSourceWebLink);
        }

        public KarmaAkashicRecord RemoveKarmaFromAvatar(IAvatarDetail avatar, KarmaTypeNegative karmaType, KarmaSourceType karmaSourceType, string karamSourceTitle, string karmaSourceDesc, string karmaSourceWebLink)
        {
            return avatar.KarmaLost(karmaType, karmaSourceType, karamSourceTitle, karmaSourceDesc, karmaSourceWebLink);
        }

        protected void OnStorageProviderError(string endPoint, string reason, Exception errorDetails)
        {
            StorageProviderError?.Invoke(this, new AvatarManagerErrorEventArgs { EndPoint = endPoint, Reason = reason, ErrorDetails = errorDetails });
        }

        public abstract Task<IEnumerable<IAvatar>> LoadAllAvatarsAsync();
        public abstract IEnumerable<IAvatar> LoadAllAvatars();

        public abstract Task<IAvatar> LoadAvatarAsync(Guid Id);
        public abstract IAvatar LoadAvatar(Guid Id);

        public abstract Task<IAvatar> LoadAvatarAsync(string username, string password);
        public abstract IAvatar LoadAvatar(string username, string password);

        public abstract IAvatar LoadAvatar(string username);
        public abstract Task<IAvatar> LoadAvatarAsync(string username);

        public abstract Task<IAvatar> LoadAvatarForProviderKeyAsync(string providerKey);
        public abstract IAvatar LoadAvatarForProviderKey(string providerKey);

        // public abstract Task<IAvatarThumbnail> LoadAvatarThumbnailAsync(Guid id);
        //  public abstract IAvatarThumbnail LoadAvatarThumbnail(Guid id);
        public abstract IAvatarDetail LoadAvatarDetail(Guid id);
        public abstract Task<IAvatarDetail> LoadAvatarDetailAsync(Guid id);
        public abstract IEnumerable<IAvatarDetail> LoadAllAvatarDetails();
        public abstract Task<IEnumerable<IAvatarDetail>> LoadAllAvatarDetailsAsync();

        public abstract IAvatar SaveAvatar(IAvatar Avatar);
        public abstract Task<IAvatar> SaveAvatarAsync(IAvatar Avatar);
        public abstract IAvatarDetail SaveAvatarDetail(IAvatarDetail Avatar);
        public abstract Task<IAvatarDetail> SaveAvatarDetailAsync(IAvatarDetail Avatar);

        public abstract bool DeleteAvatar(Guid id, bool softDelete = true);
        public abstract Task<bool> DeleteAvatarAsync(Guid id, bool softDelete = true);
        public abstract bool DeleteAvatar(string providerKey, bool softDelete = true);
        public abstract Task<bool> DeleteAvatarAsync(string providerKey, bool softDelete = true);
        public abstract Task<ISearchResults> SearchAsync(ISearchParams searchParams);

        public abstract IHolon LoadHolon(Guid id);
        public abstract Task<IHolon> LoadHolonAsync(Guid id);

        public abstract IHolon LoadHolon(string providerKey);
        public abstract Task<IHolon> LoadHolonAsync(string providerKey);

        public abstract IEnumerable<IHolon> LoadHolonsForParent(Guid id, HolonType type = HolonType.All);
        public abstract Task<IEnumerable<IHolon>> LoadHolonsForParentAsync(Guid id, HolonType type = HolonType.All);

        public abstract IEnumerable<IHolon> LoadHolonsForParent(string providerKey, HolonType type = HolonType.All);
        public abstract Task<OASISResult<IEnumerable<IHolon>>> LoadHolonsForParentAsync(string providerKey, HolonType type = HolonType.All);

        public abstract IEnumerable<IHolon> LoadAllHolons(HolonType type = HolonType.All);
        public abstract Task<IEnumerable<IHolon>> LoadAllHolonsAsync(HolonType type = HolonType.All);

        public abstract IHolon SaveHolon(IHolon holon);
        public abstract Task<IHolon> SaveHolonAsync(IHolon holon);

        public abstract IEnumerable<IHolon> SaveHolons(IEnumerable<IHolon> holons);
        public abstract Task<IEnumerable<IHolon>> SaveHolonsAsync(IEnumerable<IHolon> holons);

        public abstract bool DeleteHolon(Guid id, bool softDelete = true);
        public abstract Task<bool> DeleteHolonAsync(Guid id, bool softDelete = true);

        public abstract bool DeleteHolon(string providerKey, bool softDelete = true);
        public abstract Task<bool> DeleteHolonAsync(string providerKey, bool softDelete = true);
    }
}

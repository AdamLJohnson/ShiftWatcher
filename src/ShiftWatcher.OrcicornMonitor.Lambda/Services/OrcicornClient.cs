using ShiftWatcher.Models;
using ShiftWatcher.OrcicornMonitor.Lambda.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftWatcher.OrcicornMonitor.Lambda.Services
{
    public interface IOrcicornClient
    {
        Task<bool> ProcessAsync(string url);
    }

    public class OrcicornClient : IOrcicornClient
    {
        private readonly IHttpWebRequest _httpWebRequest;
        private readonly IPersistantStorage _persistantStorage;
        private readonly INewShiftCodeNotifier _newShiftCodeNotifier;

        public OrcicornClient(IHttpWebRequest httpWebRequest, IPersistantStorage persistantStorage, INewShiftCodeNotifier newShiftCodeNotifier)
        {
            _httpWebRequest = httpWebRequest;
            _persistantStorage = persistantStorage;
            _newShiftCodeNotifier = newShiftCodeNotifier;
        }

        public async Task<bool> ProcessAsync(string url)
        {
            var results = await _httpWebRequest.GetAsync<ActiveCodeGroups[]>(url);
            if (results == null || results.Length == 0)
                return false;

            //check the epoc of the last update
            PersistantStorageGetResult<Generated> getResults = await _persistantStorage.GetAsync<Generated>(url);
            var newCodes = false;
            if (!getResults.IsSuccess)
            {
                await _persistantStorage.InsertAsync(url, results[0].Meta.Generated);
            }

            if (getResults.Value?.Epoch == results[0].Meta.Generated.Epoch)
                return true;

            await _persistantStorage.UpdateAsync(url, results[0].Meta.Generated);

            //if epoc is different check the codes against the DB
            foreach (var code in results[0].Codes)
            {
                PersistantStorageGetResult<CodeInfo> persistantStorageCode = await _persistantStorage.GetAsync<CodeInfo>(code.Code);
                if (!persistantStorageCode.IsSuccess)
                {
                    await _persistantStorage.InsertAsync(code.Code, code);
                    await _newShiftCodeNotifier.SendAsync(code);
                }   
            }

            return true;
        }
    }
}

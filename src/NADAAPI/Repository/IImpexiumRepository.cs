using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.Repository
{
    public interface IImpexiumRepository
    {
        Task<dynamic> CallOrgProfile(string accessData, string OrgID);
        Task<dynamic> CallFullUserProfile(string accessData, string IndividualID,bool details);
        Task<dynamic> CallOrgRelation(string accessData, string OrgID, string relName, bool details);
        Task<string> GetImpexiumUser(string IndividualID);
        Task<string> GetImpexiumCommitteemembers(string groupCode, bool isManager);
        Task<string> GetImpexiumAccessToken();
        string ReadfileAndReturnString();
        Task<string> ValidateToken();
        void WriteToFile(string DataTobewritten);
     

        Dictionary<string, object> GetDictionary(object input);

        List<object> GetList(object input);

        Dictionary<string, object> ParseJsonToDictionary(string input);


        List<object> ParseJsonToList(string input);

        void WriteError(string errorMessage, string methodName, string errorType = "Ektron", string additionalParameters = "", bool sendEmail = true);


    }
}

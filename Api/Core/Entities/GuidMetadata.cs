using Newtonsoft.Json.Linq;

namespace Api.Core.Entities
{
    public class GuidMetadata
    {
        public Guid Guid { get; set; }
        public string? Metadata { get; set; }
        public long CreatedAt { get; set; }
        public long ExpiredAt { get; set; }

        public long UpdatedAt { get; set; }

        /******************************************************************************/
        /*WasExpired                                                                  */
        /*Check Guid's expire                                                         */
        /******************************************************************************/
        public bool WasExpired()
        {
            long currentDateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return ExpiredAt - currentDateTime < 0;
        }
        /******************************************************************************/
        /*ToJsonString                                                                */
        /*Formatt Metadata to a JsonString                                            */
        /******************************************************************************/
        public string ToJsonString()
        {
            var jObject = new JObject();
            if (!string.IsNullOrWhiteSpace(Metadata))
            {
                jObject = JObject.Parse(Metadata);
            }
            jObject["expire"] = ExpiredAt.ToString();
            jObject["guid"] = Guid.ToString("N").ToUpper();
            return jObject.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}

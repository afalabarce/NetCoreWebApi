using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NetCoreWebApi.Models
{
    [DataContract(Name= "WebApiUser")]
    public class WebApiUser
    {
        [Key, DataMember(Name = "Id")]
        public int Id { get; set; }        
        [DataMember(Name = "Login")]
        public string Login { get; set; }
        [DataMember(Name = "FullName")]
        public string FullName { get; set; }
        [IgnoreDataMember]
        public string Password { get; set; }
        [IgnoreDataMember]
        public string WebApiApp { get; set; }
        
        public WebApiUser()
        {
        }
    }
}


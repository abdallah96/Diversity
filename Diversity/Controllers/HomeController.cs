using AirtableApiClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;

namespace Diversity.Controllers
{
    
    public class HomeController : Controller
    {
        static string AirtableApiKey = "keylaGU2v6mUyG56F";
        static string AirtableBaseId = "appOPEcVghry00Fuq";

        AirtableBase airtableDb = new AirtableBase(AirtableApiKey, AirtableBaseId);
        public ActionResult Index()
        {
            return View();
        }

        private static string[] employeeFields = { "Age", "LGBTQIA", "HasDisability", "ServeMilitary", "Gender", "Ethnicity", "FamilyStatus", "Location", "Language" };
        private List<Dictionary<string, object>> GetAirEmployeeList()
        {
            var allEmployees = new List<AirtableRecord>();
            string offset = null, errorMessage = null;

            do
            {
                var task = Task.Run(async () => await airtableDb.ListRecords("Employee", offset, employeeFields ));
                AirtableListRecordsResponse response = task.WaitAndUnwrapException();

                if(response.Success)
                {
                    allEmployees.AddRange(response.Records.ToList());
                    offset = response.Offset;
                }
                else if (response.AirtableApiError is AirtableApiException)
                {
                    errorMessage = response.AirtableApiError.ErrorMessage;
                    break;
                }
                else
                {
                    errorMessage = "Unknown error";
                    break;
                }
            }
            while (offset != null);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return null;
            }
            else
            {
                var result = allEmployees.Select(emp => emp.Fields).ToList();
                return result;
            }
        }

        private List<string> GetAirEthnicitiesList()
        {
            var allEthnicities = new List<AirtableRecord>();
            string offset = null, errorMessage = null;

            do
            {
                var task = Task.Run(async () => await airtableDb.ListRecords("Ethnicity"));
                AirtableListRecordsResponse response = task.WaitAndUnwrapException();

                if (response.Success)
                {
                    allEthnicities.AddRange(response.Records.ToList());
                    offset = response.Offset;
                }
                else if (response.AirtableApiError is AirtableApiException)
                {
                    errorMessage = response.AirtableApiError.ErrorMessage;
                    break;
                }
                else
                {
                    errorMessage = "Unknown error";
                    break;
                }
            }
            while (offset != null);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return null;
            }
            else
            {
                var result = allEthnicities.Select(emp => emp.Fields).Select(field => (string)field["Name"]).ToList();
                return result;
            }
        }

        public string GetAirEmployees()
        {
            var employees = GetAirEmployeeList();
            if (employees == null)
            {
                return "List of employees could not be retrieved";
            }
            return JsonConvert.SerializeObject(employees);
        }

        public string GetPercentages() 
        {
            var employees = GetAirEmployeeList();
            if(employees == null)
            {
                return "List of employees could not be retrieved";
            }
            var noOfEmployees = employees.Count();
            var percentages = new
            {
                lqbgqia = (float)(employees.Where(emp => (string)emp["LGBTQIA"] == "checked").Count()) / noOfEmployees,
                disability = (float)(employees.Where(emp => (string)emp["HasDisability"] == "checked").Count()) / noOfEmployees,
                military = (float)(employees.Where(emp => (string)emp["ServeMilitary"] == "checked").Count()) / noOfEmployees,
                language = (float) (employees.Where(emp => ((JArray)emp["Language"]).Count()>0)).Count()/noOfEmployees,
            };

            return JsonConvert.SerializeObject(percentages);
        }

        public string GetNumbers()
        {
            var ethnicities = GetAirEthnicitiesList();
            var employees = GetAirEmployeeList();
            if (employees == null)
            {
                return "List of employees could not be retrieved";
            }
            var numbers = new
            {
                gender = new List<Object>
                {
                    new { name = "male", value = (float)(employees.Where(emp => (String)emp["Gender"] == "Male").Count()) / employees.Count() },
                    new { name = "female", value = (float)(employees.Where(emp => (String)emp["Gender"] == "Female").Count()) / employees.Count() },
                    new { name = "other", value = (float)(employees.Where(emp => (String)emp["Gender"] == "Agender").Count()) / employees.Count() }

                },

                ethnicity = new List<Object>(),
                location = new List<Object>
                {
                    new { name = "NorthAmerica", value = (float)(employees.Where(emp => (String)emp["Location"] == "NorthAmerica").Count()) / employees.Count() },
                    new { name = "Europe", value = (float)(employees.Where(emp => (String)emp["Location"] == "Europe").Count()) / employees.Count() },
                    new { name = "Asia", value = (float)(employees.Where(emp => (String)emp["Location"] == "Asia").Count()) / employees.Count() },
                    new { name = "Noma", value = (float)(employees.Where(emp => (String)emp["Location"] == "Noma").Count()) / employees.Count() },
                    new { name = "Australasia", value = (float)(employees.Where(emp => (String)emp["Location"] == "Australasia").Count()) / employees.Count() },
                    new { name = "SouthAmerica", value = (float)(employees.Where(emp => (String)emp["Location"] == "CentralorSouthAmerica").Count()) / employees.Count() },
                    new { name = "Africa", value = (float)(employees.Where(emp => (String)emp["Location"] == "Africa").Count()) / employees.Count() },

                },

                familyStatus = new List<Object>
                {
                    new { name = "Partnered", value = (float)(employees.Where(emp => (String)emp["FamilyStatus"] == "Partnered Parent/Legal Garant").Count()) / employees.Count() }, 
                    new { name = "No Children", value = (float)(employees.Where(emp => (String)emp["FamilyStatus"] == "No Children").Count()) / employees.Count() },
                    new { name = "Unknown", value = (float)(employees.Where(emp => (String)emp["FamilyStatus"] == "Unknown").Count()) / employees.Count() },
                },
            
                age = new List<Object>
                {
                    new { name = "[0, 18]", value = (float)(employees.Where(emp => Int32.Parse((string)emp["Age"]) > 0 && Int32.Parse((string)emp["Age"]) <= 18).Count()) / employees.Count() },
                    new { name = "[18, 25]", value = (float)(employees.Where(emp => Int32.Parse((string)emp["Age"]) > 18 && Int32.Parse((string)emp["Age"]) <= 25).Count()) / employees.Count() },
                    new { name = "[35, 45]", value = (float)(employees.Where(emp => Int32.Parse((string)emp["Age"]) > 35 && Int32.Parse((string)emp["Age"]) <= 45).Count()) / employees.Count() },
                    new { name = "[45, 55]", value = (float)(employees.Where(emp => Int32.Parse((string)emp["Age"]) > 45 && Int32.Parse((string)emp["Age"]) <= 55).Count()) / employees.Count() },
                    new { name = "[55, 65]", value = (float)(employees.Where(emp => Int32.Parse((string)emp["Age"]) > 55 && Int32.Parse((string)emp["Age"]) <= 65).Count()) / employees.Count() },
                    new { name = "[65, Inf]", value = (float)(employees.Where(emp => Int32.Parse((string)emp["Age"]) > 65).Count()) / employees.Count() },
                }
            };

           
            for (int i=0; i<ethnicities.Count(); i++)
            {
                numbers.ethnicity.Add(new {
                    name = ethnicities[i],
                    value = (float)(employees.Where(emp => (String)emp["Ethnicity"] == ethnicities[i]).Count()) / employees.Count()
                });
            }

            return JsonConvert.SerializeObject(numbers);
        }


    }
}
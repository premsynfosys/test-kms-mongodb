using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public enum KmsKeyLocation
        {
            AWS,
            Azure,
            GCP,
            Local
        }


        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {

            Post(null);
        }


        [HttpPost]
        public Task<ActionResult<TestModel>> Post(TestModel mdl)
        {
            try
            {
                var connectionString = "mongodb+srv://sbsadmin:sbsadmin@cluster0.kcbfl.mongodb.net";
                //var connectionString = "mongodb://localhost:27017/";
                var keyVaultNamespace = CollectionNamespace.FromFullName("encryption.__keyVault");

                var kmsKeyHelper = new KmsKeyHelper(
                    connectionString: connectionString,
                    keyVaultNamespace: keyVaultNamespace);

                var autoEncryptionHelper = new AutoEncryptionHelper(
                    connectionString: connectionString,
                    keyVaultNamespace: keyVaultNamespace);
                string kmsKeyIdBase64;


                kmsKeyIdBase64 = kmsKeyHelper.CreateKeyWithAwsKmsProvider();
                autoEncryptionHelper.EncryptedWriteAndRead(kmsKeyIdBase64, KmsKeyLocation.AWS);
              //  autoEncryptionHelper.QueryWithNonEncryptedClient();



                return null;
            }
            catch (Exception ex)
            {

                throw;
            }

        }



    }



}

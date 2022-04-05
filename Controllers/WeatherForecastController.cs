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
        private const string LocalMasterKey = "Mng0NCt4ZHVUYUJCa1kxNkVyNUR1QURhZ2h2UzR2d2RrZzh0cFBwM3R6NmdWMDFBMUN3YkQ5aXRRMkhGRGdQV09wOGVNYUMxT2k3NjZKelhaQmRCZGJkTXVyZG9uSjFk";
        public enum KmsKeyLocation
        {
            AWS,
            Azure,
            GCP,
            Local
        }


        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
         


        }


        [HttpPost]
        public Task<ActionResult<TestModel>> Post(TestModel mdl)
        {
            try
            {

                var localMasterKey = Convert.FromBase64String(LocalMasterKey);

                var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
                var localKey = new Dictionary<string, object>
                {
                    { "key", localMasterKey }
                };
                kmsProviders.Add("local", localKey);

                var keyVaultNamespace = CollectionNamespace.FromFullName("admin.datakeys");
                var keyVaultMongoClient = new MongoClient();
                var clientEncryptionSettings = new ClientEncryptionOptions(
                    keyVaultMongoClient,
                    keyVaultNamespace,
                    kmsProviders);

                var clientEncryption = new ClientEncryption(clientEncryptionSettings);
                var dataKeyId = clientEncryption.CreateDataKey("local", new DataKeyOptions(), CancellationToken.None);
                var base64DataKeyId = Convert.ToBase64String(GuidConverter.ToBytes(dataKeyId, GuidRepresentation.Standard));
                clientEncryption.Dispose();

                var collectionNamespace = CollectionNamespace.FromFullName("test.coll");

                var schemaMap = $@"{{
                    properties: {{
                        encryptedField: {{
                            encrypt: {{
                                keyId: [{{
                                    '$binary' : {{
                                        'base64' : '{base64DataKeyId}',
                                        'subType' : '04'
                                    }}
                                }}],
                            bsonType: 'string',
                            algorithm: 'AEAD_AES_256_CBC_HMAC_SHA_512-Deterministic'
                            }}
                        }}
                    }},
                    'bsonType': 'object'
                }}";
                var autoEncryptionSettings = new AutoEncryptionOptions(
                    keyVaultNamespace,
                    kmsProviders,
                    schemaMap: new Dictionary<string, BsonDocument>()
                    {
                        { collectionNamespace.ToString(), BsonDocument.Parse(schemaMap) }
                    });
                var clientSettings = new MongoClientSettings
                {
                    AutoEncryptionOptions = autoEncryptionSettings
                };
                var client = new MongoClient(clientSettings);
                var database = client.GetDatabase("test");
                
                var collection = database.GetCollection<BsonDocument>("coll");

                collection.InsertOne(new BsonDocument("encryptedField", "123456789"));

                var result = collection.Find(FilterDefinition<BsonDocument>.Empty).First();
                Console.WriteLine(result.ToJson());


                //var connectionString = "mongodb://localhost:27017";// "mongodb+srv://sbsadmin:sbsadmin@cluster0.kcbfl.mongodb.net";
                //                                                   //var connectionString = "mongodb://localhost:27017/";
                //var keyVaultNamespace = CollectionNamespace.FromFullName("encryption.__keyVault");

                //var kmsKeyHelper = new KmsKeyHelper(
                //    connectionString: connectionString,
                //    keyVaultNamespace: keyVaultNamespace);

                //var autoEncryptionHelper = new AutoEncryptionHelper(
                //    connectionString: connectionString,
                //    keyVaultNamespace: keyVaultNamespace);
                //string kmsKeyIdBase64;


                //kmsKeyIdBase64 = kmsKeyHelper.CreateKeyWithAwsKmsProvider();
                //autoEncryptionHelper.EncryptedWriteAndRead(kmsKeyIdBase64, KmsKeyLocation.AWS);

                //autoEncryptionHelper.QueryWithNonEncryptedClient();



                return null;
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        [HttpGet]
        public Task<ActionResult<object>> Get()
        {
            try
            {
                var localMasterKey = Convert.FromBase64String(LocalMasterKey);

                var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
                var localKey = new Dictionary<string, object>
                {
                    { "key", localMasterKey }
                };
                kmsProviders.Add("local", localKey);

                var keyVaultNamespace = CollectionNamespace.FromFullName("admin.datakeys");
                var keyVaultMongoClient = new MongoClient();
                var clientEncryptionSettings = new ClientEncryptionOptions(
                    keyVaultMongoClient,
                    keyVaultNamespace,
                    kmsProviders);

                var clientEncryption = new ClientEncryption(clientEncryptionSettings);
                var dataKeyId = clientEncryption.CreateDataKey("local", new DataKeyOptions(), CancellationToken.None);
                var base64DataKeyId = Convert.ToBase64String(GuidConverter.ToBytes(dataKeyId, GuidRepresentation.Standard));
                clientEncryption.Dispose();
                var collectionNamespace = CollectionNamespace.FromFullName("test.coll");

                var schemaMap = $@"{{
                    properties: {{
                        encryptedField: {{
                            encrypt: {{
                                keyId: [{{
                                    '$binary' : {{
                                        'base64' : '{base64DataKeyId}',
                                        'subType' : '04'
                                    }}
                                }}],
                            bsonType: 'string',
                            algorithm: 'AEAD_AES_256_CBC_HMAC_SHA_512-Deterministic'
                            }}
                        }}
                    }},
                    'bsonType': 'object'
                }}";
                var autoEncryptionSettings = new AutoEncryptionOptions(
                    keyVaultNamespace,
                    kmsProviders,
                    schemaMap: new Dictionary<string, BsonDocument>()
                    {
                        { collectionNamespace.ToString(), BsonDocument.Parse(schemaMap) }
                    });
                var clientSettings = new MongoClientSettings
                {
                    AutoEncryptionOptions = autoEncryptionSettings
                };
                var client = new MongoClient(clientSettings);
                var database = client.GetDatabase("test");
                
                var collection = database.GetCollection<BsonDocument>("coll");
                var result = collection.Find(FilterDefinition<BsonDocument>.Empty).First();
                Console.WriteLine(result.ToJson());
            }
            catch
            {

            }
            return null;
        }
    }



}

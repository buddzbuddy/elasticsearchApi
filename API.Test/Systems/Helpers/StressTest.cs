using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Data.Entities;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services;
using elasticsearchApi.Services.PinGenerator.MaxCalculatorProviders;
using elasticsearchApi.Tests.Helpers;
using elasticsearchApi.Tests.Infrastructure;
using elasticsearchApi.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using SqlKata.Execution;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace elasticsearchApi.Tests.Systems.Helpers
{
    [Collection("StressSequential")]
    public class StressTest : TestUtils
    {
        public StressTest(ITestOutputHelper output) : base(output)
        {
        }
        [Fact(Skip = "Not yet ready. Should be implemented properly")]
        public void StressAddNewPerson()
        {
            /*
             * COMMENT SKIP prop of Fact to enable execution
             * REQUIREMENTS TO USE THIS TEST
             * 1 - take backup
             * 2 - run test individual
             * 3 - restore backup without tail-log, with overwrite existing database and DONT USE SINGLE USER MODE (leave false in Close all existing connections) (if restore do not works - kill connections from nrsz-test)
             * KILL CONNECTIONS from NRSZ-TEST:
             * use [master]
             * SELECT request_session_id FROM sys.dm_tran_locks 
             * WHERE resource_database_id = DB_ID('nrsz-test')
             * 
             * kill 54;
             * 
             * AND DONT FORGET TO UNCOMMENT SKIP prop of Fact to disable execution
             * 
             * LATER FEATURES:
             * Make restor through call t-sql after test done
             * RESTORE DATABASE [nrsz-test] FROM  DISK = N'/var/opt/mssql/data/nrsz-test.bak'
             * WITH REPLACE
             */
            int numberOfThreads = 50;
            var models = GenerateAddPersonModels(numberOfThreads);
            var application = ApplicationHelper.GetWebApplicationStandard();
            using var services = application.Services.CreateScope();
            var cache = services.ServiceProvider.GetRequiredService<ICacheService>();
            /*var queryFactory = services.ServiceProvider.GetRequiredService<QueryFactory>();
            var appTransaction = services.ServiceProvider.GetRequiredService<AppTransaction>();*/

            var addressRefs = cache.GetObject(CacheKeys.ADDRESS_REFS_KEY) as AddressEntity[];
            addressRefs.Should().NotBeNullOrEmpty();
            var random = new Random();
            var allTasks = new List<Task<IServiceContext>>();

            var concurrentList = new ConcurrentDictionary<Task, IServiceContext>();
            //appTransaction.DisableCommitRollback();
            //appTransaction.Transaction = queryFactory.Connection.BeginTransaction();
            try
            {
                for (int i = 0; i < numberOfThreads; i++)
                {
                    var addressRef = addressRefs?[random.Next(0, addressRefs.Length - 1)];
                    addressRef.Should().NotBeNull();
                    var model = models[i];
                    _output.WriteLine($"task {i} started..");

                    //_testAddNewPersonItem(application, i, addressRef.regionNo, addressRef.districtNo, model);
                    //_testAddNewPersonItem(application, i, addressRef.regionNo, addressRef.districtNo, model);
                    var t = Task.Run(() => _testAddNewPersonItem(application, i, addressRef.regionNo, addressRef.districtNo, model));
                    //var t = Task.Run(() => _testDataService_AddNewPersonItem(application, i, addressRef.regionNo, addressRef.districtNo, model));

                    allTasks.Add(t);
                }

                Task.WaitAll(allTasks.ToArray());
                var results = allTasks.Select(x => x.Result);
                _output.WriteLine($"success - {results.Count(x => x.SuccessFlag)}; failed - {results.Count(x => !x.SuccessFlag)}");

                var successResults = results.Where(x => x.SuccessFlag).Select(x => x["ResultPIN"] as string);
                successResults.Should().NotBeNullOrEmpty();
                successResults.Count().Should().Be(successResults.Distinct().Count());
            }
            finally
            {
                //appTransaction.Transaction?.Rollback();
                //_output.WriteLine("RESTORE COMMAND STARTED IN OTHER THREAD.. PLEASE WAIT A FEW SECONDS (20 sec) to start again this test");
                //Task.Run(() => DBServeHelper.RestoreDatabaseToLastPoint());
            }
        }
        private List<addNewPersonDTO> GenerateAddPersonModels(int amount)
        {
            var nameRandomizer = new NameRandomizer();
            var numberRandomizer = new NameRandomizer(7);
            var names = nameRandomizer.Generate(NameRandomizer.GeneratorType.WORD, amount);
            var numbers = numberRandomizer.Generate(NameRandomizer.GeneratorType.NUMBER, amount);
            var birthDateRandomizer = new DateRandomizer(new DateTime(1995, 1, 1), DateTime.Today.AddDays(-2));
            var passportDateRandomizer = new DateRandomizer(new DateTime(2013, 1, 1), DateTime.Today.AddDays(-2));
            var model = new List<addNewPersonDTO>(amount);
            for (int i = 0; i < amount; i++)
            {
                var item = new addNewPersonDTO
                {
                    last_name = $"Ф {names[i]}",
                    first_name = $"И {names[i]}",
                    middle_name = $"О {names[i]}",
                    sex = Genders.MALE.GetValueId(),
                    date_of_birth = birthDateRandomizer.Generate(),
                    date_of_issue = passportDateRandomizer.Generate(),
                    issuing_authority = $"Выдан {names[i]}",
                    passportno = numbers[i],
                    familystate = FamilyStates.MARRIED.GetValueId(),
                    passportseries = "А",
                    passporttype = PassportTypes.PASSPORT.GetValueId()
                };
                model.Add(item);
            }
            return model;
        }
        
        private IServiceContext _testAddNewPersonItem(WebApplicationFactory<Program> application, int taskNo, int regionNo, int districtNo, addNewPersonDTO model)
        {
            using var services = application.Services.CreateScope();
            var sut = services.ServiceProvider.GetRequiredService<IAddNewPersonFacade>();
            var context = sut.AddNewPerson(model, in regionNo, in districtNo);
            _output.WriteLine($"task {taskNo} finished - {context.SuccessFlag}\n errorMessage: {context.ErrorMessages.ToStringJoin()}");
            return context;
        }
        private IServiceContext _testDataService_AddNewPersonItem(WebApplicationFactory<Program> application, int taskNo, int regionNo, int districtNo, addNewPersonDTO model)
        {
            using var services = application.Services.CreateScope();
            var sut = services.ServiceProvider.GetRequiredService<IDataService>();
            IServiceContext context = new ServiceContext();
            sut.AddNewPerson(model, regionNo, districtNo, ref context);
            _output.WriteLine($"task {taskNo} finished - {context.SuccessFlag}\n errorMessage: {context.ErrorMessages.ToStringJoin()}");
            return context;
        }

    }
}

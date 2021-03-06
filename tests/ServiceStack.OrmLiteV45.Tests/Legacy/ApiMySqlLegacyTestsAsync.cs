﻿using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.OrmLite.Legacy;
using ServiceStack.OrmLite.Tests.Shared;

namespace ServiceStack.OrmLite.Tests.Legacy
{
    public class ApiMySqlLegacyTestsAsync
        : OrmLiteTestBase
    {
        private IDbConnection db;

        [SetUp]
        public void SetUp()
        {
            SuppressIfOracle("MySql tests");
            db = CreateMySqlDbFactory().OpenDbConnection();
            db.DropAndCreateTable<Person>();
            db.DropAndCreateTable<PersonWithAutoId>();
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        [Test]
        public async Task API_MySql_Legacy_Examples_Async()
        {
            await db.SelectAsync<Person>(q => q.Where(x => x.Age > 40).OrderBy(x => x.Id));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT `Id`, `FirstName`, `LastName`, `Age` \nFROM `Person`\nWHERE (`Age` > @0)\nORDER BY `Id`"));

            await db.SelectAsync<Person>(q => q.Where(x => x.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT `Id`, `FirstName`, `LastName`, `Age` \nFROM `Person`\nWHERE (`Age` > @0)"));

            await db.SingleAsync<Person>(q => q.Where(x => x.Age == 42));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT `Id`, `FirstName`, `LastName`, `Age` \nFROM `Person`\nWHERE (`Age` = @0)\nLIMIT 1"));

            await db.SelectFmtAsync<Person>("Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT `Id`, `FirstName`, `LastName`, `Age` FROM `Person` WHERE Age > 40"));

            await db.SelectFmtAsync<Person>("SELECT * FROM Person WHERE Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age > 40"));

            await db.SelectFmtAsync<EntityWithId>(typeof(Person), "Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT `Id` FROM `Person` WHERE Age > 40"));

            await db.SingleFmtAsync<Person>("Age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT `Id`, `FirstName`, `LastName`, `Age` FROM `Person` WHERE Age = 42"));

            await db.ScalarFmtAsync<int>("SELECT COUNT(*) FROM Person WHERE Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age > 40"));

            await db.ColumnFmtAsync<string>("SELECT LastName FROM Person WHERE Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age = 27"));

            await db.ColumnDistinctFmtAsync<int>("SELECT Age FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age FROM Person WHERE Age < 50"));

            await db.LookupFmtAsync<int, string>("SELECT Age, LastName FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age, LastName FROM Person WHERE Age < 50"));

            await db.DictionaryFmtAsync<int, string>("SELECT Id, LastName FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Id, LastName FROM Person WHERE Age < 50"));

            await db.ExistsFmtAsync<Person>("Age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT `Id`, `FirstName`, `LastName`, `Age` FROM `Person` WHERE Age = 42"));
            await db.ExistsFmtAsync<Person>("SELECT * FROM Person WHERE Age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age = 42"));

            var rowsAffected = await db.ExecuteNonQueryAsync("UPDATE Person SET LastName={0} WHERE Id={1}".SqlFmt("WaterHouse", 7));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE Person SET LastName='WaterHouse' WHERE Id=7"));

            await db.InsertOnlyAsync(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, q => q.Insert(p => new { p.FirstName, p.Age }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO `PersonWithAutoId` (`FirstName`,`Age`) VALUES ('Amy',27)"));

            await db.InsertOnlyAsync(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, q => db.From<PersonWithAutoId>().Insert(p => new { p.FirstName, p.Age }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO `PersonWithAutoId` (`FirstName`,`Age`) VALUES ('Amy',27)"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ", LastName = "Hendo" }, q => q.Update(p => p.FirstName));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE `Person` SET `FirstName`=@0"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ" }, q => q.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE `Person` SET `FirstName`=@1 WHERE (`FirstName` = @0)"));

            await db.UpdateFmtAsync<Person>(set: "FirstName = {0}".SqlFmt("JJ"), where: "LastName = {0}".SqlFmt("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE `Person` SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));

            await db.UpdateFmtAsync(table: "Person", set: "FirstName = {0}".SqlFmt("JJ"), where: "LastName = {0}".SqlFmt("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE `Person` SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));

            await db.DeleteFmtAsync<Person>("Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM `Person` WHERE Age = 27"));

            await db.DeleteFmtAsync(typeof(Person), "Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM `Person` WHERE Age = 27"));

            await db.DeleteAsync<Person>(q => q.Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM `Person` WHERE (`Age` = @0)"));

            await db.DeleteFmtAsync<Person>(where: "Age = {0}".SqlFmt(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM `Person` WHERE Age = 27"));

            await db.DeleteFmtAsync(table: "Person", where: "Age = {0}".SqlFmt(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM `Person` WHERE Age = 27"));
        }
    }
}
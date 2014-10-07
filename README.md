Seekwell
======

Stupid simple database access for .net 2.0 and up.  Given a sql command and optional parameters the `Query<T>` and `QueryAsync<T>` methods will return an `IEnumerable<T>` and `Task<IEnumerable<T>>` respectively.  `ExecuteScalar` and `ExecuteScalarAsync` are useful for fire and forget queries and also for inserting data and returning a `scope_identity`.

Install
=======

To install Seekwell, run one of the following commands in the Package Manager Console

    PM> Install-Package Seekwell.Async
	
If you're in a pre .net 4.5 project run this command instead

	PM> Install-Package Seekwell


### Quick Start Usage

```csharp

var command = new Command();
var allPeople = await command.QueryAsync<Person>("SELECT * FROM People with (nolock)");
```

#### With Parameters
```csharp
var males = await command.QueryAsync<Person>(
    "SELECT * FROM People with (nolock) where Gender = @Gender",
    new { Gender = "M" });
```

#### Inserts
```csharp
var person = new Person
{
    Name = "John Smith",
    Gender = "M"
};
person.Id = await command.ExecuteScalarAsync(
    @"Insert into People (Name, Gender) Values(@Name, @Gender); 
        select Scope_identity();",
    new {
        Name = person.Name,
        Gender = person.Gender
    });
```

### Performance Metrics

**500 iterations querying for 1 row with 5 columns (using parameters)**

| Provider | Time |
| :--------- | ------------: |
ADO | 62ms
__Seekwell__ | 117ms
Dapper | 126ms
EntityFramework | 389ms

**500 iterations querying for 45 rows with 5 columns**

| Provider | Time |
| :--------- | ------------: |
ADO | 125ms
Dapper | 163ms
__Seekwell__ | 220ms
EntityFramework | 376ms

### Can I do non async?
Absolutely.  Seekwell is sensitive to the fact that real world projects often have red tape involved in upgrading .net versions.  That's why the main Seekwell project is written in 2.0 and the async operations are provided as extension methods in a 4.5 project.
```csharp
var people = command.Query<Person>("SELECT * FROM People with (nolock)");
```

### What is the default timeout?
The default timeout for a query is 30 seconds.  You can change that at anytime by setting the Timeout property on the Command object.

### What about IDisposable?
No part of this code requires us to implement IDisposable which means you don't have to worry about `using` statements.


### How does it know my connection string?
While you certainly can pass a connection string into the Command constructor, the default constructor pulls the first connection string found in your config file where the name isn't "LocalSqlServer".  This was done to reduce machine.config connection string frustration.

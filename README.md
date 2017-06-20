# iiuwr-bachelor-thesis

The topic of my bachelor's thesis was writing a library that uses the abstract syntax trees provided by the [re-linq library](https://relinq.codeplex.com/) to translate LINQ queries to PostgreSQL queries, query a PostgreSQL database and convert the result into an entity. In other words, an ORM utility between .NET and PostgreSQL. If you want a deeper explanation on how it works, you can read about it in detail in the ```iithesis.pdf``` document inside ```Thesis.Document``` directory, which is my thesis paper (it's written in Polish).

# Disclaimer

If you are looking for a good LINQ provider for PostgreSQL, **you should definitely not** use mine - this program is nothing but a proof of concept and you probably are looking for a full-featured and faster alternative, like commercial [Devart's LinqConnect](https://www.devart.com/dotconnect/postgresql/articles/tutorial_linq.html), or open-source [LINQ to DB](https://github.com/linq2db/linq2db).

# Used technologies and stuff

Framework: [.NET Core](https://www.microsoft.com/net/core). Database: [PostgreSQL](https://www.postgresql.org/).

Libraries: [re-linq](https://relinq.codeplex.com/), [Dapper](https://github.com/StackExchange/Dapper). For testing and benchmarking: [Xunit](https://xunit.github.io/), [Npgsql](http://www.npgsql.org/), [BenchmarkDotNet](http://benchmarkdotnet.org/). They all are licensed under their respective open licenses.

Sample database used in tests: Northwind. It is included in the ```Thesis.Migrations``` folder.
# iiuwr-thesis

The topic of my bachelor's thesis was writing a library that uses the abstract syntax trees provided by the [re-linq library](https://relinq.codeplex.com/) to translate LINQ queries to PostgreSQL queries, query a PostgreSQL database and convert the result into an entity. In other words, an ORM utility between .NET and PostgreSQL. If you want a deeper explanation on how it works, you can read about it in detail in the ```iithesis.pdf``` document inside ```Thesis.Document``` directory, which is my thesis paper (it's written in Polish).

# Disclaimer

If you are looking for a good LINQ provider for PostgreSQL, **you should definitely not** use mine - this program is nothing but a proof of concept and you probably are looking for a full-featured and faster alternative, like [Shaolinq](https://github.com/tumtumtum/Shaolinq).

# Used technologies and stuff

Framework: [.NET Core](https://www.microsoft.com/net/core). Database: [PostgreSQL](https://www.postgresql.org/).

Libraries: [re-linq](https://relinq.codeplex.com/), [Npgsql](http://www.npgsql.org/), [NUnit](https://www.nunit.org/). They all are licensed under their respective open licenses, which I'm too lazy to rewrite here.

Sample databases used in tests: Northwind, II (also used at my university's database class). They are both included in ```Thesis.Samples``` folder.
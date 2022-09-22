# MartenDB Demo

This project is being used to play with MartenDb.

- Setup for clean architecture, using WebAPI and MediatR
  - only scaffolding; nothing implemented
- Tests using fluent assertions, xunit, and xunit dependency injection

## Test

The EntryPoint is `UnitTest1.cs`, `Test1`.

At this time, all it does is:

- create a person aggregate
- call some methods on the aggregate
- write to MartenDb
- retrieve from MartenDb

## Projections

There are 2 projections in the `startup.cs`

- `PersonProjectionAggregation` - A simple projection that is a flattened representation of the person aggregate. `Marriage`, which is an object with a date and name in the aggregate, is flattened to a boolean in the projection. The projection is written to the db as a document.
- `PersonTableProjectAggregation` - the same, except it projects to a flat table rather than a document.
  - not working yet

[MartenDb recently added support for flat tables](https://jeremydmiller.com/2022/07/25/projecting-marten-events-to-a-flat-table/). It was added in version 5.8; current version is 5.9.

## Prerequisites

- .net6
- docker

## Setup

```bash
git clone https://github.com/jayallard/martendb-demo.git
cd martendb-demo/dev

# start the docker containers
docker-compose up -d

# build
cd ..
dotnet build

# run the test
dotnet test
```

If all went well, the test passed.

You can look at the postgresql database: `localhost:49095`, User=`postgres`, Password=`secret789@@`

### PgAdmin

The docker-compose includes the PGAdmin website, which is a postgres version of Microsoft SSMS.

The credentials are to log into the application itself, not a database.

- URL: `https://localhost:5050`
- User: `user@awesome.com`
- Password: `password`

Once logged into the app, you can register the database in the other container.

Database

- Server Name: `mdb-postgres` (this is the container name, thus, it's name on the docker network)
- User: `postgres`
- Passsword: `secret789@@` - per the `.env` file
- Port: `5432`

TODO: Determine if the server creation can be automated so that we dont' have to do it manually every time we create a new container.

NOTE: Any tool can be used. (I (Jay) use JetBrains DataGrip)

## CQRS Overview

Command Query Responsibility Separation is based on the principle of WORM: Most applications are Write Once, Read Many.

The application is splitup into COMMANDS and QUERIES.

- Commands - mutate the data. Commands are executed by domain logic. Aggregates and domain services are used to validate the operations against an aggregate. If successful, the aggreate is saved to the store. Commands are the gatekeepers: they enforce the domain rules
- Queries - get data - queries do not use the domain code; they simply retrieve data. The queries do not execute business logic or complex rules; they simply run a query.

Because the data is usually written once and read many times, CQRS encourages us to tune the operations separately. The queries do no need to pull from the same table that the domain logic writes to. Instead, they query from views or, even better, materialized views (aka projections).

The projections are tailored to the use cases so that when the query executes, it doesn't have to do much work. We should be able to easily add projections to satisfy new use cases, without having to alter the underlying data model.

CQRS Technologies (.NET), Non-Event Sourcing

- commands are typically handled by EF CORE; EF Core allows for the mapping of aggregates to a relational db. The aggregates, and domain services, handle all of the logic
- queries are typically handled by DAPPER, which is much ligheter. DAPPER is a mini-orm. For queries, it is faster than EF CORE, an simpler. (Although, EF Core has been improving, and it declares itself a contender against DAPPER)

## Event Sourcing Overview

Domain Driven Design (DDD) is focused on the business logic of the application. All manipulation of data is done via Aggregates and Domain Services, which are have tight control of the data.

Typical usage, in code:

- create an aggregate
- perform operations either on the aggregate directly, or via domain services
  - invalid operations are immediately stopped; the aggregate is never permitted to become invalid
- save the aggregate

Then...

- load an aggreate by id:  `repo.GetPerson(id)`
- manipulate via methods
- save the aggregate

The loading and saving of the data may be done using an ORM (such as EF Core), or any other means. When using an ORM, it is the state of the object that is saved.

- create person
- set birthday
- get married

This will result in the creation of a row: firstname=,lastname=,birthday=,marriage date=,spouse name=

Each time the person is updated, the row is updated. The row represents the current state of data.

Event sourcing works differently. Instead of storing the state of data, it stores the events. In this example, 3 events occurred: create, set birthday, get married. Using ES, the 3 events are stored rather than the resulting state. To get to current state, simply replay the events into the aggregate. (This may raise the concern about there being many events that must be read to load a simple object; ES frameworks handle that by creating "snapshots" periodically, so that only the most recent events have to be replayed)

## Unit of Work

The Unit of Work Pattern involves

- change tracking - as an object (aggregate) is manipulated, it tracks the changes so that it can efficiently write updates to the database when it is saved
- concurrency management - resolve data conflicts when multiple processes are working on the same data
- it's more than just working with the database; it's not a wrapper for a transaction. Additionally, it is responsible for other things that need to be done when saving an aggregate, such as publish domain events to Kafka

EF Core is both a Repository and a Unit of Work. It handles all of the above. It provides hooks so that when the aggregate is saved, you can do other things, such as publish events to Kafka. (Of course, this introduce an issue. What if the db save works but the publish fails? Inconsistent system. Another topic for another day: Transactional Outbox Pattern)

## Bring it all together - CQRS-ES

If you choose to use both CQRS and Event Sourcing, you have chosen CQRS-ES.

- commands manipulate the aggregate
- the aggregate is saved as a list of events - the source of truth
- the events may be projected to 0 or more views
- queries consume the views

An interesting thing about Unit of Work when using CQRS: The list of events vastly simplifies the "change tracking" feature of uow. Nothing needs to analyze or track the aggregate to determine what changed so that the db can be optimally updated. Instead, the changes are explicitly listed. The only thing that has to save is the events that occurred since the aggregate was loaded.

## MartenDB

This project is exploring use of MartenDB in a CQRS-ES app. MartenDB uses POSTGRES as a document db, and also as an event-sourcing db. It provides a client API that hides all of the implementation details, leaving the user to work with only events and documents.

MartenDb supports projections. As data changes in the event stream, the projections are updated. The projections are coded by the developer, and executed by MartenDb.

Each projection is assigned an update strategy

- inline - the projection is updated in the same transaction as the database write
- live - the projection is updated on demand (presumably, projection data can be retrieved via the api? not yet explored)
- async - eventual consistency - a daemon updates the projections

Projections may be dropped and recreated, such as for bug fixes or significan tweaks. The full capabilities and limitations of this hasn't yet been explored.

## Observations

Event Sourcing is appropriate for projects whose focus is on the domain code: business rules, validations, etc.

If the focus of the application is domain code, then Event Sourcing is a worthy pursuit. There is benefit to a system that is based on the events that occurred, rather than the result of the things that occurred. It's a full audit, built-in.

However, it is a different approach than a lot of people are accustomed to. It requires that we think about the application from the perspective of the domain, not the database (which is also a key factor of Clean Architecture and DDD)

Using these concepts, the database isn't important to the design. The domain defines Repositories and Queries, etc, and the system is built on the abstractions, not the implementations. The implementations are plugged-in.

Clean Architecture

- domain defines the interfaces
- infrastructure implements them
- domain has no knowledge of the implementations

## Summary

Key considerations:

- do we want the center of our app to be the domain logic or the database?
- are we willing to figure out new things? If we proceed with MartenDb, then we'll need to adjust our mindset. The raw-data is a list of events; projections will be updated asyncronously (eventually consistent is the new norm)
  - MartenDb projections, until recently, were exclusively documents. It now supports flat tables.
- how much of a domain is there?
  - "DDD is for hard problems. Not every problem is hard." - I forget where I read that, but it stuck with me.

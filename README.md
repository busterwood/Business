# Business application development

> The way business applications are written is wrong.

All business applications we have come across are written in general purpose languages (Java, C#, etc) which, because of their general nature, encourage developers to mix:

* pure business logic - the high level tasks the business wants to happen
* technical concerns - threading, handling technical failure
* low level details - how business task X is implemented, e.g. a calculation

We propose a new (programming) language for business logic is needed to enforce this separation of concerns, it will:

* have named `operations` (or tasks), but not define what those tasks do
* have control structures for branching, e.g. `if`
* have control structures for iteration, e.g. `foreach`
* _not_ have low level details, e.g. no arithmetic, no string library, no comparison operators
* _not_ have types, e.g. no integers, no string, no floating point numbers

We believe that _taking away_ the low level details is essential to focusing on the business process.

We believe that this will allow the business process to evolve easily over time and make implementing process change much easier.

## Business Data Model

Business applications always work with data and are defined in terms of the data model.  For example, financial trading applications always have orders, fills, bookings and settlements.  

The business data model includes essential business information and relationsjips, but no accidental complexity.  For example, we know a `fill` has a `price` and `quantity` and belongs to an `order`.

### Workflow?

Isn't this proposal just talking about workflow systems?  

No, workflow systems are built for long running business processes (hours, days) and often focus on the persistence of tasks between these processes.  Our proposal is for a more abstract business language, and maybe one application of this _could_ be a workflow system, but we propose all business systems can be built better using a separate business lanaguage.

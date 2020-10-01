# IQueryable Expression Examples

Examples for the presentation _Inside IQueryable: The Power of .NET Expressions_.

The `.ipynb` extension is for Jupyter notebooks. To run these locally, follow the instructions
at [.NET Interactive](https://github.com/dotnet/interactive).

The notebooks demonstrate:

1. Introduction to expressions
2. Example of using expressions to construct a new type instance
3. Advanced expressions (combining multiple branches and using expression blocks)
4. Introduction to queries

The remaining projects are runnable as is.

## Constructor Performance

The `CtorPerformance` project is a benchmark showing how long it takes to create new 
instances using various methods. The best way to see the results is to navigate to the project
root folder and type:

`dotnet run --configuration release` 

The project should be run in Release mode.

## Blazor App

This is a sample app showing how expressions can be serialized using the
[ExpressionPowerTools](https://github.com/jeremylikness/ExpressionPowerTools) package.
Set the `.Server` project for startup to run. It will automatically generate and seed the
initial Sqlite database.

## QueryMutators

This application shows several ways to mutate queries to perform tasks such as:

- Enforce limits on the query return ("guard rails")
- Intercept predicates in the tree
- Evaluate ALL predicates against ALL values to debug why some are `true` and others are `false`.
-  
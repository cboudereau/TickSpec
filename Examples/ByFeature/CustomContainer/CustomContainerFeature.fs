module ByFeature.CustomContainer.Feature

open Autofac
open System
open System.Reflection
open TickSpec
open Xunit

/// Creates a IServiceProvider to be used for a single Scenario run
let createServiceProvider : unit -> IServiceProvider =
    let concreteTypesSource = Features.ResolveAnything.AnyConcreteTypeNotAlreadyRegisteredSource()
    let builder = new ContainerBuilder()
    builder.RegisterSource concreteTypesSource
    let container = builder.Build()

    fun () ->
        let scope = container.BeginLifetimeScope();
        { new obj()
            interface IServiceProvider with
                member __.GetService(serviceType) =
                    scope.Resolve(serviceType)
            interface IDisposable with
                member __.Dispose() =
                    scope.Dispose() }

let source = AssemblyStepDefinitionsSource(Assembly.GetExecutingAssembly(), createServiceProvider)
let scenarios resourceName = source.ScenariosFromEmbeddedResource resourceName |> MemberData.ofScenarios

[<Theory; MemberData("scenarios", "Shelter.feature")>]
let Shelter(scenario : Scenario) = scenario.Action.Invoke()
﻿using Akka.Actor;
using Akka.DI.Core;
using Castle.Windsor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akka.DI.CastleWindsor
{
    public class WindsorDependencyResolver : IDependencyResolver
    {
        private IWindsorContainer container;
        private ConcurrentDictionary<string, Type> typeCache;
        private ActorSystem system;

        public WindsorDependencyResolver(IWindsorContainer container, ActorSystem system)
        {
            if (system == null) throw new ArgumentNullException("system");
            if (container == null) throw new ArgumentNullException("container");
            this.container = container;
            typeCache = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            this.system = system;
            this.system.AddDependencyResolver(this);
        }

      
        public Type GetType(string actorName)
        {
 
            typeCache.
                TryAdd(actorName,
                        actorName.GetTypeValue() ??
                        container.
                        Kernel.
                        GetAssignableHandlers(typeof(object)).
                        Where(handler => handler.ComponentModel.Name.Equals(actorName, StringComparison.InvariantCultureIgnoreCase)).
                        Select(handler => handler.ComponentModel.Implementation).
                        FirstOrDefault());

            return typeCache[actorName];
        }

        public Func<ActorBase> CreateActorFactory(string actorName)
        {
            return () => (ActorBase)container.Resolve(GetType(actorName));
        }

        public void Create<TActor>(string name) where TActor : ActorBase
        {
            system.ActorOf(system.GetExtension<DIExt>().Props(typeof(TActor).Name), name);
        }

    }
    internal static class Extensions
    {
        public static Type GetTypeValue(this string typeName)
        {
            var firstTry = Type.GetType(typeName);
            Func<Type> searchForType = () =>
            {
                return
                AppDomain.
                    CurrentDomain.
                    GetAssemblies().
                    SelectMany(x => x.GetTypes()).
                    Where(t => t.Name.Equals(typeName)).
                    FirstOrDefault();
            };
            return firstTry ?? searchForType();
        }

    }
}

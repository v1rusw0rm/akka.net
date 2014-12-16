﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.DI.Core
{
    public interface IDependencyResolver
    {
        Type GetType(string actorName);
        Func<ActorBase> CreateActorFactory(string actorName);
        void Create<TActor>(string name) where TActor : ActorBase;
    }
}

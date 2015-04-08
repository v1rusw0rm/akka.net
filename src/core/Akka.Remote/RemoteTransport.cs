﻿//-----------------------------------------------------------------------
// <copyright file="RemoteTransport.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;

namespace Akka.Remote
{
    /// <summary>
    /// INTERNAL API.
    /// 
    /// The remote transport is responsible for sending and receiving messages.
    /// Each transport has an address, which it should provide in Serialization.CurrentTransportInformation (thread-local)
    /// while serializing ActorReferences (which might also be part of messages). This address must
    /// be available (i.e. fully initialized) by the time the first message is received or when the Start() method
    /// returns, whichever happens first.
    /// </summary>
    public abstract class RemoteTransport
    {
        protected RemoteTransport(ExtendedActorSystem system, RemoteActorRefProvider provider)
        {
            System = system;
            Provider = provider;
        }

        protected ExtendedActorSystem System { get; private set; }
        public RemoteActorRefProvider Provider { get; private set; }

        /// <summary>
        /// Addresses to be used in <see cref="RootActorPath"/> of refs generated for this transport.
        /// </summary>
        public abstract ISet<Address> Addresses { get; }

        /// <summary>
        /// The default transport address of the <see cref="ActorSystem"/>. 
        /// This is the listen address of the default transport.
        /// </summary>
        public abstract Address DefaultAddress { get; }

        /// <summary>
        /// When true, some functionality will be turned off for security purposes
        /// </summary>
        protected bool UseUntrustedMode { get; set; }
        public bool logRemoteLifeCycleEvents { get; protected set; }

        /// <summary>
        /// A logger that can be used to log issues that may occur
        /// </summary>
        public ILoggingAdapter Log { get; protected set; }

        /// <summary>
        /// Start up the transport, i.e. enable incoming connections
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Shuts down the remoting
        /// </summary>
        /// <returns>A Task that can be waited on until shutdown is complete</returns>
        public abstract Task Shutdown();

        /// <summary>
        /// Sends the given message to the recipient, supplying <see cref="sender"/> if any.
        /// </summary>
        public abstract void Send(object message, IActorRef sender, RemoteActorRef recipient);

        /// <summary>
        /// Sends a management command to the underlying transport stack. The call returns with a Task that
        /// indicates if the command was handled successfully or dropped.
        /// </summary>
        /// <param name="cmd">a Command message to send to the transport</param>
        /// <returns>A task that indicates when the message was successfully handled or dropped</returns>
        public abstract Task<bool> ManagementCommand(object cmd);

        /// <summary>
        /// Resolves the correct local address to be used for contacting the given remote address
        /// </summary>
        /// <param name="remote">The remote address</param>
        /// <returns>the local address to be used for the given remote address</returns>
        public abstract Address LocalAddressForRemote(Address remote);

        /// <summary>
        /// Marks a remote system as out of sync and prevents reconnects until the quarantine timeout elapses.
        /// </summary>
        /// <param name="address">Address of the remote system to be quarantined</param>
        /// <param name="uid">UID of the remote system; if the uid is not defined it will not be a strong quarantine but the current
        /// endpoint writer will be stopped (dropping system messages) and the address will be gated.</param>
        public abstract void Quarantine(Address address, int? uid);
    }

    /// <summary>
    /// Represents a general failure within a <see cref="RemoteTransport"/>, such as
    /// the inability to start, wrong configuration, etc...
    /// </summary>
    public class RemoteTransportException : AkkaException
    {
        public RemoteTransportException(string message, Exception cause = null)
            : base(message, cause)
        {
        }

        protected RemoteTransportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

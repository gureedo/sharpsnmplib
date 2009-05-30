/*
 * Created by SharpDevelop.
 * User: lextm
 * Date: 2008/5/1
 * Time: 18:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Lextm.SharpSnmpLib.Security;

namespace Lextm.SharpSnmpLib.Messaging
{
    /// <summary>
    /// GET response message.
    /// </summary>
    public class GetResponseMessage : ISnmpMessage
    {
        private Header _header;
        private SecurityParameters _parameters;
        private Scope _scope;
        private readonly VersionCode _version;
        private ProviderPair _record;

        /// <summary>
        /// Creates a <see cref="GetResponseMessage"/> with all contents.
        /// </summary>
        /// <param name="requestId">Request ID.</param>
        /// <param name="version">Protocol version.</param>
        /// <param name="community">Community name.</param>
        /// <param name="variables">Variables.</param>
        public GetResponseMessage(int requestId, VersionCode version, OctetString community, IList<Variable> variables)
        {
            if (version == VersionCode.V3)
            {
                throw new ArgumentException("Please use overload constructor for v3", "version");
            }

            _version = version;
            _header = Header.Empty;
            _parameters = new SecurityParameters(null, null, null, community, null, null);
            GetResponsePdu pdu = new GetResponsePdu(
                new Integer32(requestId),
                ErrorCode.NoError,
                new Integer32(0),
                variables);
            _scope = new Scope(null, null, pdu);
            _record = ProviderPair.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetResponseMessage"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="requestId">The request id.</param>
        /// <param name="messageId">The message id.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="record">The record.</param>
        public GetResponseMessage(VersionCode version, int requestId, int messageId, OctetString userName, IList<Variable> variables, ProviderPair record)
        {            
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            _version = version;
            _record = record;
            Levels recordToSecurityLevel = record.ToSecurityLevel();
            recordToSecurityLevel |= Levels.Reportable;
            byte b = (byte)recordToSecurityLevel;
            _header = new Header(new Integer32(messageId), new Integer32(0xFFE3), new OctetString(new byte[] { b }), new Integer32(3));
            _parameters = new SecurityParameters(OctetString.Empty, new Integer32(0), new Integer32(0), userName, OctetString.Empty, OctetString.Empty);
            GetRequestPdu pdu = new GetRequestPdu(
                requestId,
                ErrorCode.NoError,
                0,
                variables);
            _scope = new Scope(OctetString.Empty, OctetString.Empty, pdu);
        }

        internal GetResponseMessage(VersionCode version, Header header, SecurityParameters parameters, Scope scope, ProviderPair record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            _version = version;
            _header = header;
            _parameters = parameters;
            _scope = scope;
            _record = record;
        }                
        
        /// <summary>
        /// Sends this <see cref="GetRequestMessage"/> and handles the response from agent.
        /// </summary>
        /// <param name="receiver">The receiver.</param>
        public void Send(EndPoint receiver)
        {
            Send(receiver, Helper.GetSocket(receiver));
        }

        /// <summary>
        /// Sends this <see cref="GetRequestMessage"/> and handles the response from agent.
        /// </summary>
        /// <param name="receiver">The receiver.</param>
        /// <param name="socket">The socket.</param>
        public void Send(EndPoint receiver, Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            
            socket.SendTo(ToBytes(), receiver);
        }
        
        /// <summary>
        /// Error status.
        /// </summary>
        public ErrorCode ErrorStatus
        {
            get { return _scope.Pdu.ErrorStatus.ToErrorCode(); }
        }
        
        /// <summary>
        /// Error index.
        /// </summary>
        public int ErrorIndex
        {
            get { return _scope.Pdu.ErrorIndex.ToInt32(); }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public VersionCode Version
        {
            get { return _version; }
        }

        /// <summary>
        /// Request ID.
        /// </summary>
        public int RequestId
        {
            get { return _scope.Pdu.RequestId.ToInt32(); }
        }
        
        /// <summary>
        /// Variables.
        /// </summary>
        public IList<Variable> Variables
        {
            get { return _scope.Pdu.Variables; }
        }
        
        /// <summary>
        /// PDU.
        /// </summary>
        public ISnmpPdu Pdu 
        {
            get { return _scope.Pdu; }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public SecurityParameters Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>The scope.</value>
        public Scope Scope
        {
            get { return _scope; }
        }
        
        /// <summary>
        /// Converts to byte format.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return Helper.PackMessage(_version, _record.Privacy, _header, _parameters, _scope).ToBytes();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="GetResponseMessage"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GET response message: version: " + _version + "; " + _parameters.UserName + "; " + _scope.Pdu;
        }
    }
}
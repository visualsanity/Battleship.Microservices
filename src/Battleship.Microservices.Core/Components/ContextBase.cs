﻿namespace Battleship.Microservices.Core.Components
{
    using System;

    using Battleship.Microservices.Core.Messages;
    using Battleship.Microservices.Core.Models;
    using Battleship.Microservices.Core.Utilities;

    using Microsoft.AspNetCore.Mvc;

    using Newtonsoft.Json;

    public abstract class ContextBase : ControllerBase
    {
        #region Fields

        private readonly IMessagePublisher messagePublisher;

        #endregion

        #region Constructors

        protected ContextBase(IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
        }

        #endregion

        #region Properties

        protected string AuditQueue => "AuditLog";

        #endregion

        #region Methods

        protected async void Log(Exception exp, AuditType auditType = AuditType.Error)
        {
            if (exp != null)
            {
                string error = $"{exp.Message}\n{exp.StackTrace}";
                string message = this.MarshalMessage(error, auditType);
                await this.messagePublisher.PublishMessageAsync(message, this.AuditQueue);
            }
        }

        protected async void Log(string content, AuditType auditType = AuditType.Info)
        {
            if (!string.IsNullOrEmpty(content))
            {
                string message = this.MarshalMessage(content, auditType);
                await this.messagePublisher.PublishMessageAsync(message, this.AuditQueue);
            }
        }

        protected async void Log(string content, string queue, AuditType auditType = AuditType.Info)
        {
            if (!string.IsNullOrEmpty(content)) await this.messagePublisher.PublishMessageAsync(content, queue);
        }

        private string MarshalMessage(string content, AuditType auditType)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(content))
            {
                Audit audit = new Audit
                  {
                      Content = content,
                      Timestamp = DateTime.Now,
                      AuditType = auditType
                  };

                result = JsonConvert.SerializeObject(audit);
            }

            return result;
        }

        #endregion
    }
}
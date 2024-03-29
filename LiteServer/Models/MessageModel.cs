﻿using LiteServer.IO.DAL.Model;
using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class MessageModel
    {
        public static MessageModel Create(Message message)
        {
            return new MessageModel()
            {
                Id = message.Id,
                Text = message.Text,
                GroupId = message.GroupId,
                Time = message.Date,
                UserUuid = message.UserUuid,
                AttachmentType = message.AttachmentType,
                AttachmentReference = message.AttachmentReference
            };
        }

        [JsonProperty("id")]
        public long Id;
        [JsonProperty("text")]
        public string Text;
        [JsonProperty("user")]
        public Guid UserUuid;
        [JsonProperty("groupId")]
        public uint GroupId;
        [JsonProperty("time")]
        public DateTime Time;
        [JsonProperty("attachment_type")]
        public byte AttachmentType;
        [JsonProperty("attachment")]
        public long AttachmentReference;
    }
}

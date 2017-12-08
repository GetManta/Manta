﻿using System;

namespace Manta
{
    public struct MessageRecord
    {
        public MessageRecord(Guid messageId, int contractId, byte[] payload)
        {
            MessageId = messageId;
            ContractId = contractId;
            Payload = payload;
        }

        public Guid MessageId { get; }
        public int ContractId { get; }
        public byte[] Payload { get; }
    }
}
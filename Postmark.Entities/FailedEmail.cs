using Postmark.Entities.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.Entities
{
    public class FailedEmail : IEntity
    {
        public int Id { get; set; }
    }
}

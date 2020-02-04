using AutoMapper;
using Postmark.WebAPI.Models;
using System;
using System.Collections.Generic;

namespace Postmark.WebAPI.Models
{
    public class Header
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Attachment
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
        public string ContentID { get; set; }
    }

    public class Metadata
    {
        public string color { get; set; }
        public string client_id { get; set; }
    }

    public class SingleEmail : EmailRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Tag { get; set; }
        public string HtmlBody { get; set; }
        public string TextBody { get; set; }
        public string ReplyTo { get; set; }
        public List<Header> Headers { get; set; }
        public bool TrackOpens { get; set; }
        public string TrackLinks { get; set; }
        public string UniqueEmailID { get; set; }
        public List<Attachment> Attachments { get; set; }
        public Metadata Metadata { get; set; }

        public SingleEmail()
        {

        }

        public SingleEmail(SingleEmail singleEmail)
        {
         
        }
    }


    
}

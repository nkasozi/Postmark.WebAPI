using Postmark.WebAPI.Models;
using System;

namespace Postmark.Shared.Interfaces
{
    public interface IRule<T, TResult>
    {
        TResult RunRuleChecks(ref T obj);
    }
}

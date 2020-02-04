using Postmark.WebAPI.Models;
using System;

namespace Postmark.Shared.Interfaces
{
    public interface IRule<T,TResult>
    {
        PreconditionResult RunPreConditionChecks(ref T obj);
        TResult RunRuleChecks(ref T obj);
    }
}

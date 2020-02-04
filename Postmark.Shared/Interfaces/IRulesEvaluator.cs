namespace Postmark.Shared.Interfaces
{
    public interface IRulesEvaluator<TRuleEvaluator,TRule,TClass,TResult>
    {
        void Add(TRule rule);

        TResult RunAllRules(ref TClass tclass);

        TRuleEvaluator WithRule(TRule rule);

        TRuleEvaluator WithRules(TRule[] rules);
    }
}

using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ContainsFullTextParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;
    private readonly Expression _expressionTreeRoot;

    public ContainsFullTextParser (Expression expressionTreeRoot, WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _expressionTreeRoot = expressionTreeRoot;
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (MethodCallExpression methodCallExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return CreateContainsFulltext (methodCallExpression, (string) ((ConstantExpression) methodCallExpression.Arguments[1]).Value, fieldDescriptorCollection);
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((MethodCallExpression) expression, fieldDescriptors);
    }

    public bool CanParse (Expression expression)
    {
      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
      {
        if (methodCallExpression.Method.Name == "ContainsFulltext")
          return true;
      }
      return false;
    }

    private BinaryCondition CreateContainsFulltext (MethodCallExpression expression, string pattern, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new BinaryCondition (_parserRegistry.GetParser (expression.Arguments[0]).Parse (expression.Arguments[0], fieldDescriptorCollection), new Constant (pattern), BinaryCondition.ConditionKind.ContainsFulltext);
    }

  }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class ContainsFulltextParserTest
  {
    [Test]
    public void ParseContainsFulltext ()
    {
      var methodName = "ContainsFulltext";
      var pattern = "Test";
      CheckParsingOfContainsFulltext (methodName, pattern);
    }

    public static bool Contains () { return true; }

    private static void CheckParsingOfContainsFulltext (string methodName, string pattern)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (Remotion.Data.Linq.ExtensionMethods.ExtensionMethods).GetMethod (methodName),
          Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "s"), typeof (Student).GetProperty ("First")),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Constant ("Test");

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (queryModel, StubDatabaseInfo.Instance, new JoinedTableContext ());
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (queryModel, resolver));

      //MethodCallExpressionParser parser = new MethodCallExpressionParser (queryModel.GetExpressionTree (), parserRegistry);
      ContainsFullTextParser parser = new ContainsFullTextParser (queryModel.GetExpressionTree (), parserRegistry);
      

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      ICriterion actualCriterion = parser.Parse (methodCallExpression, fieldCollection);
      ICriterion expectedCriterion = new BinaryCondition (new Column (new Table ("studentTable", "s"), "FirstColumn"), new Constant (pattern), BinaryCondition.ConditionKind.ContainsFulltext);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }
  }
}
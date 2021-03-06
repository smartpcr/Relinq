// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class SelectManyQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void MultiFromsAndWhere ()
    {
      var expression = MixedTestQueryGenerator.CreateMultiFromWhereQuery (QuerySource, QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);

      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckConstantQuerySource (additionalFromClause.FromExpression, QuerySource);

      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Name == "Garcia");

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, queryModel.MainFromClause, s1 => s1);
    }

    [Test]
    public void TwoFromsWithMemberAccess ()
    {
      var expression = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySource (IndustrialSectorQuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, IndustrialSectorQuerySource);

      var memberFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Restaurant, IEnumerable<Cook>> (memberFromClause.FromExpression, mainFromClause, sector => sector.Cooks);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, memberFromClause, s1 => s1);
    }

    [Test]
    public void GeneralSelectMany ()
    {
      var expression = FromTestQueryGenerator.CreateMultiFromQuery (QuerySource, QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s1"));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));

      Assert.That (queryModel.BodyClauses[0], Is.InstanceOf (typeof (AdditionalFromClause)));
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      Assert.That (additionalFromClause.ItemName, Is.EqualTo ("s2"));
      CheckConstantQuerySource (additionalFromClause.FromExpression, QuerySource);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, queryModel.MainFromClause, s1 => s1);
    }

    [Test]
    public void SelectMany_InSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateThreeFromWhereQuery (QuerySource, QuerySource, QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);

      var additionalFromClause1 = (AdditionalFromClause) queryModel.BodyClauses[0];
      Assert.That (additionalFromClause1.ItemName, Is.EqualTo ("s2"));
      
      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.FirstName == "Hugo");

      var additionalFromClause2 = (AdditionalFromClause) queryModel.BodyClauses[2];
      Assert.That (additionalFromClause2.ItemName, Is.EqualTo ("s3"));
    }

    [Test]
    public void WhereSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhereQuery (QuerySource, QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s1"));
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (2));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Name == "Garcia");

      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[1];
      Assert.That (additionalFromClause.ItemName, Is.EqualTo ("s2"));
      CheckConstantQuerySource (additionalFromClause.FromExpression, QuerySource);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, mainFromClause, s1 => s1);
    }

    [Test]
    public void WhereAndSelectMannyWithProjection ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhereQueryWithProjection (QuerySource, QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s1"));
      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (2));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Name == "Garcia");

      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[1];
      CheckConstantQuerySource (additionalFromClause.FromExpression, QuerySource);
      
      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, string> (selectClause.Selector, (AdditionalFromClause) queryModel.BodyClauses.Last(), s2 => s2.Name);
    }

    [Test]
    public void SelectMany_WithoutResultSelector ()
    {
      var expression = QuerySource.SelectMany (c => c.Assistants).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);

      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, IEnumerable<Cook>> (additionalFromClause.FromExpression, mainFromClause, c => c.Assistants);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, additionalFromClause, x => x);
    }
  }
}

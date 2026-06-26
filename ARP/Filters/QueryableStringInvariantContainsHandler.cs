using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ARP.Filters
{
    public class QueryableStringInvariantContainsHandler : QueryableStringOperationHandler
    {
        public QueryableStringInvariantContainsHandler(InputParser inputParser)
            : base(inputParser) { }

        protected override int Operation => DefaultFilterOperations.Contains;

        public override Expression HandleOperation(
            QueryableFilterContext context,
            IFilterOperationField field,
            IValueNode value,
            object? parsedValue)
        {
            Expression property = context.GetInstance();

            if (parsedValue is string str)
            {
                // Usa ILike do Postgres — case-insensitive nativo
                return Expression.Call(
                    typeof(NpgsqlDbFunctionsExtensions),
                    nameof(NpgsqlDbFunctionsExtensions.ILike),
                    Type.EmptyTypes,
                    Expression.Property(null, typeof(EF), nameof(EF.Functions)),
                    property,
                    Expression.Constant($"%{str}%")
                );
            }

            throw new InvalidOperationException();
        }
    }
}

using Dapper;
using HotChocolate.Language;
using System.Reflection;

namespace ARP
{
    public static class GraphQLSqlSortParser
    {
        public static void ApplySort<T>(
            SqlBuilder builder,
            string alias,
            IValueNode? sort,
            IEnumerable<string>? allowedFields = null)
        {
            if (sort is null)
                return;

            if (sort is ListValueNode list)
            {
                foreach (var item in list.Items)
                {
                    if (item is not ObjectValueNode obj)
                        throw new GraphQLException("Sort inválido. Esperado objeto.");

                    ApplyObjectSort<T>(builder, alias, obj, allowedFields);
                }

                return;
            }

            if (sort is ObjectValueNode singleObj)
            {
                ApplyObjectSort<T>(builder, alias, singleObj, allowedFields);
                return;
            }

            throw new GraphQLException("Sort inválido. Esperado objeto ou lista.");
        }

        private static void ApplyObjectSort<T>(
            SqlBuilder builder,
            string alias,
            ObjectValueNode obj,
            IEnumerable<string>? allowedFields)
        {
            foreach (var field in obj.Fields)
            {
                var fieldName = field.Name.Value;

                ValidateFieldExists<T>(fieldName);

                if (allowedFields is not null &&
                    !allowedFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
                {
                    throw new GraphQLException(
                        $"Ordenação não permitida para o campo '{fieldName}'.");
                }

                if (field.Value is not EnumValueNode enumVal)
                    throw new GraphQLException(
                        $"Direção inválida para '{fieldName}'. Use ASC ou DESC.");

                var direction = enumVal.Value.ToUpper();

                if (direction is not ("ASC" or "DESC"))
                    throw new GraphQLException(
                        $"Direção inválida '{direction}'. Permitido: ASC ou DESC.");

                builder.OrderBy($"{alias}.{fieldName} {direction}");
            }
        }

        private static void ValidateFieldExists<T>(string fieldName)
        {
            var exists = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Any(p => p.Name.Equals(fieldName,
                    StringComparison.OrdinalIgnoreCase));

            if (!exists)
                throw new GraphQLException(
                    $"Campo '{fieldName}' não existe na entidade.");
        }
    }
}

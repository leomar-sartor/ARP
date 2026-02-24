using Dapper;
using HotChocolate.Language;
using System.Reflection;

namespace ARP
{
    public static class GraphQLSqlFilterParser
    {
        private static readonly HashSet<string> SupportedOperations =
        [
            "eq","neq","contains","startsWith","endsWith","in"
        ];

        public static void ApplyFilter<T>(
            SqlBuilder builder,
            string alias,
            IValueNode? where,
            DynamicParameters parameters)
        {
            if (where is null)
                return;

            if (where is not ObjectValueNode obj)
                throw new GraphQLException("Filtro inválido. Esperado objeto.");

            var condition = ParseObject<T>(alias, obj, parameters);

            if (!string.IsNullOrWhiteSpace(condition))
                builder.Where(condition);
        }

        private static string ParseObject<T>(
            string alias,
            ObjectValueNode obj,
            DynamicParameters parameters)
        {
            var conditions = new List<string>();

            foreach (var field in obj.Fields)
            {
                var name = field.Name.Value;

                if (IsLogicalOperator(name))
                {
                    conditions.Add(ParseLogical<T>(alias, name, field.Value, parameters));
                    continue;
                }

                ValidateFieldExists<T>(name);

                if (field.Value is not ObjectValueNode operations)
                    throw new GraphQLException($"Campo '{name}' precisa conter operações.");

                foreach (var op in operations.Fields)
                {
                    if (!SupportedOperations.Contains(op.Name.Value))
                        throw new GraphQLException(
                            $"Operação '{op.Name.Value}' não suportada.");

                    var condition = BuildCondition(
                        alias,
                        name,
                        op,
                        parameters);

                    conditions.Add(condition);
                }
            }

            return string.Join(" AND ", conditions);
        }

        private static string ParseLogical<T>(
            string alias,
            string logicName,
            IValueNode value,
            DynamicParameters parameters)
        {
            var logic = logicName.Equals("and", StringComparison.OrdinalIgnoreCase)
                ? "AND"
                : logicName.Equals("or", StringComparison.OrdinalIgnoreCase)
                    ? "OR"
                    : throw new GraphQLException("Operador lógico inválido.");

            var nestedConditions = new List<string>();

            if (value is ListValueNode list)
            {
                foreach (var item in list.Items.OfType<ObjectValueNode>())
                {
                    var nested = ParseObject<T>(alias, item, parameters);
                    nestedConditions.Add($"({nested})");
                }
            }
            else if (value is ObjectValueNode single)
            {
                var nested = ParseObject<T>(alias, single, parameters);
                nestedConditions.Add($"({nested})");
            }
            else
            {
                throw new GraphQLException(
                    $"Operador '{logicName}' precisa ser objeto ou lista.");
            }

            return string.Join($" {logic} ", nestedConditions);
        }

        private static string BuildCondition(
            string alias,
            string fieldName,
            ObjectFieldNode operation,
            DynamicParameters parameters)
        {
            var paramName = $"p_{Guid.NewGuid():N}";
            var value = GetValue(operation.Value)
                ?? throw new GraphQLException(
                    $"Valor inválido para operação '{operation.Name.Value}'.");

            switch (operation.Name.Value)
            {
                case "eq":
                    parameters.Add(paramName, value);
                    return $"{alias}.{fieldName} = @{paramName}";

                case "neq":
                    parameters.Add(paramName, value);
                    return $"{alias}.{fieldName} <> @{paramName}";

                case "contains":
                    parameters.Add(paramName, $"%{value}%");
                    return $"{alias}.{fieldName} LIKE @{paramName}";

                case "startsWith":
                    parameters.Add(paramName, $"{value}%");
                    return $"{alias}.{fieldName} LIKE @{paramName}";

                case "endsWith":
                    parameters.Add(paramName, $"%{value}");
                    return $"{alias}.{fieldName} LIKE @{paramName}";

                case "in":
                    if (operation.Value is not ListValueNode list)
                        throw new GraphQLException(
                            "Operação 'in' precisa receber uma lista.");

                    var values = list.Items.Select(GetValue).ToArray();
                    parameters.Add(paramName, values);
                    return $"{alias}.{fieldName} IN @{paramName}";

                default:
                    throw new GraphQLException(
                        $"Operação '{operation.Name.Value}' não suportada.");
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

        private static bool IsLogicalOperator(string name)
            => name.Equals("and", StringComparison.OrdinalIgnoreCase)
            || name.Equals("or", StringComparison.OrdinalIgnoreCase);

        private static object? GetValue(IValueNode value)
            => value switch
            {
                StringValueNode s => s.Value,
                IntValueNode i => i.ToInt32(),
                FloatValueNode f => f.ToDouble(),
                BooleanValueNode b => b.Value,
                EnumValueNode e => e.Value,
                _ => null
            };
    }
}


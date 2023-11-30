using System.Linq.Expressions;
using System.Text;

namespace BusinessRulesManager.Services
{
    public class ExpressionToStringVisitor : ExpressionVisitor
    {
        private StringBuilder _builder = new StringBuilder();
        private bool _isRoot = true;

        public override Expression Visit(Expression node)
        {
            if (node == null) return node;

            if (_isRoot && node is LambdaExpression)
            {
                var lambda = (LambdaExpression)node;
                _isRoot = false;

                Visit(lambda.Parameters.Single()); // assuming single parameter for simplicity
                _builder.Append(" => ");
                Visit(lambda.Body);
            }
            else
            {
                base.Visit(node);
            }
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _builder.Append("(");
            Visit(node.Left);
            _builder.Append($" {GetOperator(node.NodeType)} ");
            Visit(node.Right);
            _builder.Append(")");
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _builder.Append(node.Value);
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _builder.Append(node.Name);
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null)
            {
                Visit(node.Expression);
                _builder.Append($".{node.Member.Name}");
            }
            else
            {
                // Static member access
                _builder.Append($"{node.Member.DeclaringType.Name}.{node.Member.Name}");
            }
            return node;
        }

        private string GetOperator(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.AndAlso: return "&&";
                case ExpressionType.OrElse: return "||";
                case ExpressionType.Equal: return "==";
                case ExpressionType.NotEqual: return "!=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThanOrEqual: return "<=";
                // Add more cases for other operators as needed
                default: return "";
            }
        }

        public string GetStringRepresentation()
        {
            return _builder.ToString();
        }
    }

}

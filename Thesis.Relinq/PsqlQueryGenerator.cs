using Remotion.Linq;
using Thesis.Relinq.PsqlQueryGeneration;

namespace Thesis.Relinq
{
    public static class PsqlQueryGenerator
    {
        public static PsqlCommandData GeneratePsqlQuery(QueryModel queryModel) =>
            PsqlGeneratingQueryModelVisitor.GeneratePsqlQuery(queryModel);
    }
}
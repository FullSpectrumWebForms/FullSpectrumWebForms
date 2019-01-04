using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls.ServerSide
{
    public class Dataset<DataContext, DataType> : Html.Div where DataContext : IDisposable
    {
        public override string ControlType => nameof(Html.Div);
        public class DatasetRow
        {
            public Dictionary<string, object> ColValues = new Dictionary<string, object>();

        }


        private Func<DataContext> GetDataContext;
        public Dataset<DataContext, DataType> Context(Func<DataContext> getContext)
        {
            GetDataContext = getContext;
            return this;
        }

        private Func<DataContext, IQueryable<DataType>> GetQuery;
        public Dataset<DataContext, DataType> Query( Func< DataContext, IQueryable<DataType>> query )
        {
            GetQuery = query;
            return this;
        }

    }
}

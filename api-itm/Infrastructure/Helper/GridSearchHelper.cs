using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Helper
{
    public class GridSearchHelper
    {
        //    // ============== UI: search ==============
        //    public TextBox _txtSearch;
        //    public Button _btnClearSearch;

        //    // data cache
        //    private object _rowsData;                 // List<anon>
        //    private object _allRowsData;    // original full List<anon>
        //    private Type _rowItemType;
        //    private string _lastSortProp;            // last sorted property

        //    private ListSortDirection _lastSortDir = ListSortDirection.Descending;

        //    public void ApplySearchFilter()
        //    {
        //        if (_allRowsData == null || _rowItemType == null) return;

        //        var q = _txtSearch.Text?.Trim();
        //        IEnumerable<object> items = ((IEnumerable)_allRowsData).Cast<object>();

        //        if (!string.IsNullOrEmpty(q))
        //        {
        //            var props = _rowItemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //            items = items.Where(o =>
        //            {
        //                foreach (var p in props)
        //                {
        //                    var v = p.GetValue(o, null);
        //                    if (v == null) continue;

        //                    string s = v is DateTime dt ? dt.ToString("yyyy-MM-dd") : v.ToString();
        //                    if (!string.IsNullOrEmpty(s) && s.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
        //                        return true;
        //                }
        //                return false;
        //            });
        //        }

        //        // keep current sort if any
        //        if (!string.IsNullOrWhiteSpace(_lastSortProp))
        //        {
        //            var pi = _rowItemType.GetProperty(_lastSortProp, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        //            if (pi != null)
        //            {
        //                items = _lastSortDir == ListSortDirection.Descending
        //                    ? items.OrderByDescending(o => pi.GetValue(o, null), _nullsLast)
        //                    : items.OrderBy(o => pi.GetValue(o, null), _nullsLast);
        //            }
        //        }

        //        // Cast<T> + ToList<T> for anonymous type
        //        var castM = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(_rowItemType);
        //        var toList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!.MakeGenericMethod(_rowItemType);
        //        var casted = castM.Invoke(null, new object[] { items });
        //        var list = toList.Invoke(null, new object[] { casted });

        //        dgvViewSalariati.DataSource = list;
        //        _rowsData = list;

        //        RenumberRows();
        //        UpdateCounts();
        //        ApplyRowColorsByRegesId();

        //        foreach (DataGridViewColumn c in dgvViewSalariati.Columns)
        //            c.HeaderCell.SortGlyphDirection = SortOrder.None;

        //        if (!string.IsNullOrWhiteSpace(_lastSortProp))
        //        {
        //            var sortedCol = dgvViewSalariati.Columns
        //                .Cast<DataGridViewColumn>()
        //                .FirstOrDefault(c =>
        //                    string.Equals(c.DataPropertyName, _lastSortProp, StringComparison.OrdinalIgnoreCase) ||
        //                    string.Equals(c.Name, _lastSortProp, StringComparison.OrdinalIgnoreCase));
        //            if (sortedCol != null)
        //                sortedCol.HeaderCell.SortGlyphDirection =
        //                    _lastSortDir == ListSortDirection.Descending ? SortOrder.Descending : SortOrder.Ascending;
        //        }


        //    }

        //    // nulls-last comparer for object keys
        //    private static readonly IComparer<object> _nullsLast = Comparer<object>.Create((a, b) =>
        //    {
        //        if (a == null && b == null) return 0;
        //        if (a == null) return 1;    // nulls last
        //        if (b == null) return -1;
        //        if (a.GetType() == b.GetType() && a is IComparable ca) return ca.CompareTo(b);
        //        return string.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCulture);
        //    });


    }
}

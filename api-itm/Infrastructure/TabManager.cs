using System;
using System.Linq;
using System.Windows.Forms;

namespace api_itm.Infrastructure
{
    /// <summary>
    /// A helper class that manages a TabControl:
    /// - Opens a tab if it doesn't exist
    /// - Activates a tab if it already exists - the tab the currently selected one so it’s shown to the user in the TabControl
    /// - Closes a tab by key
    /// </summary>
    public class TabManager
    {
        // Reference to the TabControl that this manager will control
        private readonly TabControl _tabs;

        /// <summary>
        /// Constructor that takes the TabControl instance to manage
        /// </summary>
        public TabManager(TabControl tabs) => _tabs = tabs;

        /// <summary>
        /// Opens a new tab or activates an existing one.
        /// </summary>
        /// <param name="key">A unique name for the tab (stored in TabPage.Name).</param>
        /// <param name="createContent">A function that creates the Control to show inside the tab.</param>
        /// <param name="title">The text that appears on the tab header.</param>
        public void OpenOrActivate(string key, Func<Control> createContent, string title)
        {
            // Check if a tab with the given key already exists
            var exists = _tabs.TabPages
                              .Cast<TabPage>()
                              .FirstOrDefault(p => p.Name == key);

            if (exists != null)
            {
                // If found, just select it (bring it to front)
                _tabs.SelectedTab = exists;
                return;
            }

            // Create a new tab page
            var page = new TabPage
            {
                Name = key,   // Internal unique identifier
                Text = title  // Text shown on the tab header
            };

            // Create the content control (using the function provided by the caller)
            var content = createContent();
            content.Dock = DockStyle.Fill; // Make it fill the tab page

            // Add the content into the tab page
            page.Controls.Add(content);

            // Add the tab page to the TabControl
            _tabs.TabPages.Add(page);

            // Make the new tab the active tab
            _tabs.SelectedTab = page;
        }

        /// <summary>
        /// Closes (removes) a tab by its key.
        /// </summary>
        public void Close(string key)
        {
            // Find the tab with the given key
            var exists = _tabs.TabPages
                              .Cast<TabPage>()
                              .FirstOrDefault(p => p.Name == key);

            // If found, remove it from the TabControl
            if (exists != null)
                _tabs.TabPages.Remove(exists);
        }
    }
}

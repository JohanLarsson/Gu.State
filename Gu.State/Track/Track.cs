namespace Gu.State
{
    using System.ComponentModel;
    using System.Reflection;

    public static partial class Track
    {
        /// <summary>
        /// Creates a tracker that detects and notifies about differences for any property or subproperty of <paramref name="x"/> compared to <paramref name="y"/>
        /// </summary>
        /// <typeparam name="T">
        /// The type of <paramref name="x"/> and <paramref name="y"/>
        /// </typeparam>
        /// <param name="x">The first item to track changes for and compare to <paramref name="y"/>.</param>
        /// <param name="y">The othe item to track changes for and compare to <paramref name="x"/>.</param>
        /// <param name="referenceHandling">
        /// - Structural tracks property values for the entire graph.
        /// - References tracks only one level and uses reference equality.
        /// - Throw throws and exception if there are nested trackable types
        /// </param>
        /// <param name="bindingFlags">
        /// The <see cref="BindingFlags"/> to use when getting properties to track
        /// </param>
        /// <returns>An <see cref="DirtyTracker{T}"/> that signals on differences between in <paramref name="x"/> and <paramref name="y"/></returns>
        public static DirtyTracker<T> IsDirty<T>(
            T x,
            T y,
            ReferenceHandling referenceHandling = ReferenceHandling.Structural,
            BindingFlags bindingFlags = Constants.DefaultPropertyBindingFlags)
            where T : class, INotifyPropertyChanged
        {
            var settings = PropertiesSettings.GetOrCreate(referenceHandling, bindingFlags);
            return IsDirty(x, y, settings);
        }

        /// <summary>
        /// Creates a tracker that detects and notifies about differences for any property or subproperty of <paramref name="x"/> compared to <paramref name="y"/>
        /// </summary>
        /// <typeparam name="T">
        /// The type of <paramref name="x"/> and <paramref name="y"/>
        /// </typeparam>
        /// <param name="x">The first item to track changes for and compare to <paramref name="y"/>.</param>
        /// <param name="y">The othe item to track changes for and compare to <paramref name="x"/>.</param>
        /// <param name="settings">
        /// An instance of <see cref="PropertiesSettings"/> configuring how tracking will be performed
        /// </param>
        /// <returns>An <see cref="DirtyTracker{T}"/> that signals on differences between in <paramref name="x"/> and <paramref name="y"/></returns>
        public static DirtyTracker<T> IsDirty<T>(T x, T y, PropertiesSettings settings)
            where T : class, INotifyPropertyChanged
        {
            return new DirtyTracker<T>(x, y, settings);
        }

        /// <summary>
        /// Creates a tracker that detects and notifies about changes of any property or subproperty of <paramref name="root"/>
        /// </summary>
        /// <param name="root">The item to track changes for.</param>
        /// <param name="referenceHandling">
        /// - Structural tracks property values for the entire graph.
        /// - References tracks only one level.
        /// - Throw throws and exception if there are nested trackable types
        /// </param>
        /// <param name="bindingFlags">
        /// The <see cref="BindingFlags"/> to use when getting properties to track
        /// </param>
        /// <returns>An <see cref="IChangeTracker"/> that signals on changes in <paramref name="root"/></returns>
        public static IChangeTracker Changes(
            INotifyPropertyChanged root,
            ReferenceHandling referenceHandling = ReferenceHandling.Structural,
            BindingFlags bindingFlags = Constants.DefaultPropertyBindingFlags)
        {
            Ensure.NotNull(root, nameof(root));
            var settings = PropertiesSettings.GetOrCreate(referenceHandling: referenceHandling, bindingFlags: bindingFlags);
            return Changes(root, settings);
        }

        /// <summary>
        /// Creates a tracker that detects and notifies about changes of any property or subproperty of <paramref name="root"/>
        /// </summary>
        /// <param name="root">The item to track changes for.</param>
        /// <param name="settings">
        /// Configuration for how to track.
        /// For best performance settings should be cached between usages if anthing other than <see cref="ReferenceHandling"/> or <see cref="BindingFlags"/> is configured.
        /// </param>
        /// <returns>
        /// An <see cref="IChangeTracker"/> that signals on changes.
        /// Disposing it stops tracking.
        /// <paramref name="root"/></returns>
        public static IChangeTracker Changes(INotifyPropertyChanged root, PropertiesSettings settings)
        {
            Ensure.NotNull(root, nameof(root));
            Ensure.NotNull(settings, nameof(settings));
            return new ChangeTracker(root, settings);
        }
    }
}
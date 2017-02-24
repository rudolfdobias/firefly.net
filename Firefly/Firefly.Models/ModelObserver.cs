using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Firefly.Models
{
    public class ModelObserver
    {
        private readonly Dictionary<string, ChangeObservingArgs> _triggers = new Dictionary<string, ChangeObservingArgs>();
        public event EntityObservingArgs NotChanging;
        public event EntityObservingArgs Creating;
        public event EntityObservingArgs Updating;
        public event EntityObservingArgs Deleting;

        public void Handle(ChangeTracker tracker)
        {
            foreach (var e in tracker.Entries())
            {
                switch (e.State)
                {
                    case EntityState.Added:
                        OnCreating(e, tracker);
                        break;
                    case EntityState.Modified:
                        OnUpdating(e, tracker);
                        break;
                    case EntityState.Deleted:
                        OnDeleting(e, tracker);
                        break;
                    case EntityState.Detached:
                    case EntityState.Unchanged:
                        OnNoChange(e, tracker);
                        break;
                }
                OnAll(e, tracker);
            }
        }

        public void Register(string actionName, Func<EntityEntry, bool> filter, EntityObservingArgs action)
            => Register(new ChangeObservingArgs() {ActionName = actionName, Filter = filter, Action = action});

        public void Unregister(string actionName)
        {
            if (_triggers.ContainsKey(actionName))
            {
                _triggers.Remove(actionName);
            }
        }

        public void Register(ChangeObservingArgs args)
        {
            if (args.Action == null)
            {
                throw new ArgumentException("The parameter 'Action' cannot be null in ChangeObservingArgs");
            }
            if (args.ActionName == null)
            {
                throw new ArgumentException("The parameter 'ActionName' must be filled.");
            }
            if (_triggers.ContainsKey(args.ActionName))
            {
                return;
            }
            _triggers.Add(args.ActionName, args);
        }

        protected virtual void OnAll(EntityEntry entry, ChangeTracker tracker)
        {
            foreach (var info in _triggers.Values)
            {
                if (info.Filter == null || info.Filter.Invoke(entry))
                {
                    info.Action(entry, tracker);
                }
            }
        }

        protected virtual void OnCreating(EntityEntry e, ChangeTracker tracker)
        {
            Creating?.Invoke(e, tracker);
        }

        protected virtual void OnUpdating(EntityEntry e, ChangeTracker tracker)
        {
            Updating?.Invoke(e, tracker);
        }

        protected virtual void OnDeleting(EntityEntry e, ChangeTracker tracker)
        {
            Deleting?.Invoke(e, tracker);
        }

        protected virtual void OnNoChange(EntityEntry e, ChangeTracker tracker)
        {
            NotChanging?.Invoke(e, tracker);
        }
    }

    public delegate void EntityObservingArgs(EntityEntry entity, ChangeTracker tracker);

    public struct ChangeObservingArgs
    {
        public string ActionName { get; set; }
        public Func<EntityEntry, bool> Filter { get; set; }
        public EntityObservingArgs Action { get; set; }
    }
}
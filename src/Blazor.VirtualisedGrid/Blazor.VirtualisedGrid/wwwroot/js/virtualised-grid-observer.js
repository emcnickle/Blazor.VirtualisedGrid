// Observer storage map: gridId -> { scrollObserver, aboveObserver, belowObserver, component }
var gridObservers = new Map();

window.VirtualisedGridObserver = {
    Initialize: function (gridId, component, scrollObserverTarget, belowObserverTarget, aboveObserverTarget, isLoadOnDemand) {
        // Validate gridId parameter
        if (!gridId || typeof gridId !== 'string') {
            console.error('VirtualisedGridObserver.Initialize: gridId must be a non-empty string');
            return;
        }

        // Dispose existing observers for this grid if they exist
        if (gridObservers.has(gridId)) {
            var existingObservers = gridObservers.get(gridId);
            if (existingObservers.scrollObserver) existingObservers.scrollObserver.disconnect();
            if (existingObservers.aboveObserver) existingObservers.aboveObserver.disconnect();
            if (existingObservers.belowObserver) existingObservers.belowObserver.disconnect();
        }

        // Create new observer set for this grid
        var observers = {
            scrollObserver: null,
            aboveObserver: null,
            belowObserver: null,
            component: component
        };

        if (isLoadOnDemand === true) {
            observers.scrollObserver = new IntersectionObserver(entries => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        component.invokeMethodAsync('OnReadRows');
                    }
                })
            }, {
                rootMargin: '200px 0px', // Trigger when element is 200px from viewport
                threshold: 0.1
            });

            var scrollElements = document.getElementsByClassName(scrollObserverTarget);
            for (let i = 0; i < scrollElements.length; i++) {
                var element = scrollElements[i];
                observers.scrollObserver.observe(element);
            };
        }

        else {
            observers.aboveObserver = new IntersectionObserver(entries => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        component.invokeMethodAsync('UpdateVisibleRangeStart', entry.target.id);
                        observers.aboveObserver.unobserve(entry.target);
                    }
                })
            });
            var elements = document.getElementsByClassName(aboveObserverTarget);
            for (let i = 0; i < elements.length; i++) {
                observers.aboveObserver.observe(elements[i]);
            };

            observers.belowObserver = new IntersectionObserver(entries => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        component.invokeMethodAsync('UpdateVisibleRangeEnd', entry.target.id);
                        observers.belowObserver.unobserve(entry.target);
                    }
                })
            });

            var elements2 = document.getElementsByClassName(belowObserverTarget);
            for (let i = 0; i < elements2.length; i++) {
                observers.belowObserver.observe(elements2[i]);
            };
        }

        // Store the observer set in the map
        gridObservers.set(gridId, observers);
    }
};

// Dispose observers for a specific grid
function DisposeVirtualisedGridObserver(gridId) {
    // Validate gridId parameter
    if (!gridId || typeof gridId !== 'string') {
        console.error('DisposeVirtualisedGridObserver: gridId must be a non-empty string');
        return;
    }

    // Check if observers exist for this grid
    if (!gridObservers.has(gridId)) {
        // No observers to dispose for this grid - this is not an error
        return;
    }

    // Get and dispose observers for this specific grid
    var observers = gridObservers.get(gridId);
    if (observers.scrollObserver) observers.scrollObserver.disconnect();
    if (observers.aboveObserver) observers.aboveObserver.disconnect();
    if (observers.belowObserver) observers.belowObserver.disconnect();

    // Remove from map
    gridObservers.delete(gridId);
}


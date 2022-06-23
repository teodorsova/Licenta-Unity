mergeInto(LibraryManager.library, {
  FurnitureSpawn: function (Name, price, companyName) {
    window.dispatchReactUnityEvent(
      "FurnitureSpawn",
         Pointer_stringify(Name),
         Pointer_stringify(price),
        Pointer_stringify(companyName)
    );
  },
});
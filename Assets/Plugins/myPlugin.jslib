mergeInto(LibraryManager.library, {
  FurnitureSpawn: function (Name, price, companyName) {
    window.dispatchReactUnityEvent(
      "FurnitureSpawn",
         Name,
         Pointer_stringify(price),
        companyName
    );
  },
});
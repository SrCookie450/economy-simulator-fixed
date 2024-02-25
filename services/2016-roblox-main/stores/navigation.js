import { useState } from "react";
import { createContainer } from "unstated-next";

const NavigationStore = createContainer(() => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  return {
    isSidebarOpen,
    setIsSidebarOpen,
  }
});

export default NavigationStore;
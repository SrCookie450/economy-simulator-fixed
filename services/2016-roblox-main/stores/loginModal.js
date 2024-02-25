import { useState } from "react";
import { createContainer } from "unstated-next";

const LoginModalStore = createContainer(() => {
  const [open, setOpen] = useState(null);

  return {
    open,
    setOpen,
  }
})

export default LoginModalStore;
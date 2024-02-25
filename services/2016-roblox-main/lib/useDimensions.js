import {useEffect, useState} from "react";

const useDimensions = () => {
  const [dimensions, setDimensions] = useState({
    height: window.innerHeight,
    width: window.innerWidth
  });
  useEffect(() => {
    const f = () => {
      setDimensions({
        height: window.innerHeight,
        width: window.innerWidth
      });
    };
    window.addEventListener('resize', f);
    return () => {
      window.removeEventListener('resize', f);
    }
  }, []);

  return [dimensions];
}

export default useDimensions;
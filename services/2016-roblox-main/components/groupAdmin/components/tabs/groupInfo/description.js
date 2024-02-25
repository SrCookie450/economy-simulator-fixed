import {useEffect, useState} from "react";

const Description = props => {
  const [description, setDescription] = useState('');
  useEffect(() => {
    if (!description)
      setDescription(props.description.current || '');
  }, [props]);

  return <>
    <textarea rows={16} className='w-100 font-monospace' onChange={(v) => {
      setDescription(v.currentTarget.value);
      props.description.current = v.currentTarget.value;
    }} value={description} />
  </>
}

export default Description;
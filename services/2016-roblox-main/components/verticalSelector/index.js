import React from "react";
import { createUseStyles } from "react-jss";
import SelectorOption from "./components/selectorOption";

const useStyles = createUseStyles({
  row: {
    borderRight: '1px solid #ccc',
  },
})

/**
 * Vertical selector, as seen on "Develop" page
 * @param {{selected: string; options: {name: string; url: string; disabled?: boolean; onClick: () => void}[]}} props 
 * @returns 
 */
const VerticalSelector = props => {
  const s = useStyles();

  return <div className={'row mt-4 pe-0 ' + s.row}>
    <div className='col-12 pe-0 me-0'>
      {
        props.options.map(v => {
          return <SelectorOption key={v.name + v.url} onClick={v.onClick} name={v.name} url={v.url} selected={props.selected === v.name} disabled={v.disabled}/>
        })
      }
    </div>
  </div>
}

export default VerticalSelector;
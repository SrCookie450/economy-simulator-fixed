import React from "react";
import { createUseStyles } from "react-jss";
import MyMessagesStore from "../../../stores/myMessages";
import GenericPagination from "../../genericPagination";

const useStyles = createUseStyles({
  buttonWrapper: {

  },
})

const MessagePagination = props => {
  const s = useStyles();
  const store = MyMessagesStore.useContainer();
  if (store.messages === null) {
    return null;
  }
  const pages = Math.ceil(store.total / store.limit);
  const currentPage = Math.ceil(store.offset / store.limit) + 1;

  return <div className='row'>
    <div className='col-12 col-lg-4 offset-lg-8 mt-3 mb-2'>
      <GenericPagination
        page={currentPage}
        pageCount={pages === 0 ? 1 : pages}
        onClick={(incrementMode) => {
          return (e) => {
            e.preventDefault();
            let newPage = currentPage + incrementMode;
            if (newPage > pages || newPage <= 0) return
            store.setOffset(incrementMode === 1 ? (store.offset + store.limit) : (store.offset - store.limit));
          }
        }}
      ></GenericPagination>
    </div>
  </div>
}

export default MessagePagination;
import NextLink from 'next/link';

const Link = props => {
  return <NextLink href={props.href}>
    {props.children}
  </NextLink>
}

export default Link;
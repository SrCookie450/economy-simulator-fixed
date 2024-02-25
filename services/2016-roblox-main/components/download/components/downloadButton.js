import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
    image: {
        width: '100%',
        height: 'auto',
        maxWidth: '150px',
        maxHeight: '150px',
        margin: '0 auto',
        display: 'block',
        objectFit: 'contain',
    },
    card: {
        filter: 'grayscale(100%)',
        transition: '100ms',
        '&:hover': {
            transition: '100ms',
            filter: 'grayscale(0)',
        },
    },
});

const DownloadButton = ({ url, title, imageUrl }) => {
    const s = useStyles();

    return <div className='col-6 col-lg-4 mx-auto'>
        <div className={'card ' + s.card}>
            <div className='card-body'>
                <a href={url}>
                    <img alt={`${title} Player Icon`} src={imageUrl} className={s.image} />
                    <h4 className='text-center mt-4'>{title}</h4>
                </a>
            </div>
        </div>
    </div>
}

export default DownloadButton;
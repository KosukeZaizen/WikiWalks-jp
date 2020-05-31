import * as React from 'react';
import { Helmet } from 'react-helmet';

const PageHeader = props => {
    return (
        <Helmet>
            {
                props.title && <title>{props.title}</title>
            }
            {
                props.desc && <meta name="description" content={props.desc} />
            }
            {
                props.noindex && <meta name="robots" content="noindex" />
            }
        </Helmet>
    );
};
export default PageHeader;
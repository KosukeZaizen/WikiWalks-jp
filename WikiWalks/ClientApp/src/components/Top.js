import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { actionCreators } from '../store/WikiWalks';
//import { getEnglishDate } from '../common/functions';
//import Head from './Helmet';

class Top extends Component {

    constructor(props) {
        super(props);

        this.state = {
            categories: [],
        }
    }

    componentDidMount() {
        const getData = async () => {
            const url = `api/WikiWalks/getAllCategories`;
            const response = await fetch(url);
            const categories = await response.json();
            this.setState({ categories });
        }
        getData();
    }

    render() {
        const { categories } = this.state;
        const description = `This is a list of the Wikipedia pages about . Please check the list below to know about !`;
        const arrDesc = description.split(". ");
        const lineChangeDesc = arrDesc.map((d, i) => <span key={i}>{d}{i < arrDesc.length - 1 && ". "}<br /></span>);
        return (
            <div>
                {/*<Head
                    title={title}
                    desc={description}
                />*/}
                <h1>Welcome to Wiki Ninja!</h1>
                <br />
                <p>
                    Do you know Wikipedia? It is the best online dictionary in the world!<br />
                    This website introduces you articles of Wikipedia for each theme!
                </p>
                <br />
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th>Category Name</th>
                            <th>Number of Words</th>
                        </tr>
                    </thead>
                    <tbody>
                        {categories.length > 0 ? categories.map(category =>
                            <tr key={category.category}>
                                <td>
                                    {<Link to={"/category/" + category.category.split(" ").join("_")}>{category.category}</Link>}
                                </td>
                                <td>
                                    {category.cnt} words
                                </td>
                            </tr>
                        )
                            :
                            <tr><td>Loading...</td><td></td></tr>}
                    </tbody>
                </table>
            </div>
        );
    }
}

export default connect(
    state => state.wikiWalks,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Top);
